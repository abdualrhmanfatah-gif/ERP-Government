using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.PhysicalCounts.Commands;

[Authorize(Policy = PermissionCodes.AssetCountsExecute)]
public record StartAssetPhysicalCountCommand(int Id) : IRequest<Result<int>>;

/// <summary>
/// Freezes the count scope and generates exactly one observation line per in-scope asset
/// (FR-111 semantics, FR-112 snapshot rule, FR-116 uniqueness).
/// </summary>
public class StartAssetPhysicalCountCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<StartAssetPhysicalCountCommand, Result<int>>
{
    public async Task<Result<int>> Handle(StartAssetPhysicalCountCommand request, CancellationToken ct)
    {
        var count = await context.AssetPhysicalCounts.FindAsync([request.Id], ct);
        if (count is null)
            return Result<int>.Failure(ErrorCodes.Assets.CountNotFound, ErrorCategory.NotFound, "وثيقة الجرد غير موجودة");

        if (count.Status != CountStatus.Draft)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidCountStatus, ErrorCategory.Validation, "لا يمكن بدء الجرد إلا من حالة مسودة");

        var assetsQuery = context.Assets.AsQueryable();

        if (count.LocationId.HasValue)
            assetsQuery = assetsQuery.Where(a => a.LocationId == count.LocationId.Value);

        if (count.DepartmentId.HasValue)
            assetsQuery = assetsQuery.Where(a => a.Employee != null
                && a.Employee.OrganizationalUnitId == count.DepartmentId.Value);

        var assets = await assetsQuery
            .Select(a => new { a.Id, a.LocationId, a.EmployeeId, a.Status })
            .ToListAsync(ct);

        if (assets.Count == 0)
            return Result<int>.Failure(ErrorCodes.Assets.NoEligibleAssets, ErrorCategory.Validation, "لا توجد أصول مطابقة لنطاق الجرد");

        var lines = assets.Select(a => new AssetPhysicalCountDetail
        {
            AssetPhysicalCountId = count.Id,
            AssetId = a.Id,
            SystemLocationId = a.LocationId,
            SystemEmployeeId = a.EmployeeId,
            SystemStatus = a.Status,
            IsFound = CountFoundState.NotExamined,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        }).ToList();

        context.AssetPhysicalCountDetails.AddRange(lines);

        count.Status = CountStatus.InProgress;
        count.StartedAt = DateTime.UtcNow;
        count.CountedById ??= user.Id;
        count.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(count.Id);
    }
}

public class StartAssetPhysicalCountCommandValidator : AbstractValidator<StartAssetPhysicalCountCommand>
{
    public StartAssetPhysicalCountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف وثيقة الجرد مطلوب");
    }
}
