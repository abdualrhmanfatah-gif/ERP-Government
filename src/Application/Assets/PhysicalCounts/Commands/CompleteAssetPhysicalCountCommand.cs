using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.PhysicalCounts.Commands;

[Authorize(Policy = PermissionCodes.AssetCountsExecute)]
public record CompleteAssetPhysicalCountCommand(int Id) : IRequest<Result<int>>;

/// <summary>
/// Completes the count only when every included line has a definite state (found / not found) —
/// any line still "not examined" blocks completion (FR-114).
/// </summary>
public class CompleteAssetPhysicalCountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CompleteAssetPhysicalCountCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CompleteAssetPhysicalCountCommand request, CancellationToken ct)
    {
        var count = await context.AssetPhysicalCounts.FindAsync([request.Id], ct);
        if (count is null)
            return Result<int>.Failure(ErrorCodes.Assets.CountNotFound, ErrorCategory.NotFound, "وثيقة الجرد غير موجودة");

        if (count.Status != CountStatus.InProgress)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidCountStatus, ErrorCategory.Validation, "لا يمكن إكمال الجرد إلا من حالة قيد التنفيذ");

        var hasPendingLines = await context.AssetPhysicalCountDetails
            .AnyAsync(l => l.AssetPhysicalCountId == request.Id && l.IsFound == CountFoundState.NotExamined, ct);
        if (hasPendingLines)
            return Result<int>.Failure(ErrorCodes.Assets.CountLinesPending, ErrorCategory.Validation, "يوجد سطور لم تُفحص، أكمل فحص جميع السطور قبل الإكمال");

        count.Status = CountStatus.Completed;
        count.CompletedAt = DateTime.UtcNow;
        count.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(count.Id);
    }
}

public class CompleteAssetPhysicalCountCommandValidator : AbstractValidator<CompleteAssetPhysicalCountCommand>
{
    public CompleteAssetPhysicalCountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف وثيقة الجرد مطلوب");
    }
}
