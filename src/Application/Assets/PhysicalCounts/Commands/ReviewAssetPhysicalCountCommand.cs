using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.PhysicalCounts.Commands;

[Authorize(Policy = PermissionCodes.AssetCountsReview)]
public record ReviewAssetPhysicalCountCommand(int Id) : IRequest<Result<int>>;

/// <summary>
/// Records the reviewer sign-off. Review follows the current procedure and adds no approval cycle;
/// the reviewer identity is stored on the header (FR-118).
/// </summary>
public class ReviewAssetPhysicalCountCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ReviewAssetPhysicalCountCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ReviewAssetPhysicalCountCommand request, CancellationToken ct)
    {
        var count = await context.AssetPhysicalCounts.FindAsync([request.Id], ct);
        if (count is null)
            return Result<int>.Failure(ErrorCodes.Assets.CountNotFound, ErrorCategory.NotFound, "وثيقة الجرد غير موجودة");

        if (count.Status != CountStatus.Completed)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidCountStatus, ErrorCategory.Validation, "لا يمكن مراجعة الجرد إلا بعد إكماله");

        count.ReviewedById = user.Id;
        count.Status = CountStatus.Reviewed;
        count.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(count.Id);
    }
}

public class ReviewAssetPhysicalCountCommandValidator : AbstractValidator<ReviewAssetPhysicalCountCommand>
{
    public ReviewAssetPhysicalCountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف وثيقة الجرد مطلوب");
    }
}
