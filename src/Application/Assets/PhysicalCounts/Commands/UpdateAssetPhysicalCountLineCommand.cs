using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.PhysicalCounts.Commands;

[Authorize(Policy = PermissionCodes.AssetCountsExecute)]
public record UpdateAssetPhysicalCountLineCommand(
    int CountId,
    int LineId,
    CountFoundState IsFound,
    int? PhysicalLocationId,
    int? PhysicalEmployeeId,
    string? PhysicalStatus,
    string? DiscrepancyNotes,
    byte[] RowVersion) : IRequest<Result<int>>;

/// <summary>
/// Records a physical observation on the single line the asset already owns (FR-116):
/// the line is updated in place, duplicate insertion is impossible, the RowVersion guard
/// prevents overwriting concurrent edits, and the audit interceptor records old/new values.
/// </summary>
public class UpdateAssetPhysicalCountLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAssetPhysicalCountLineCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateAssetPhysicalCountLineCommand request, CancellationToken ct)
    {
        if (!Enum.IsDefined(request.IsFound))
            return Result<int>.Failure(ErrorCodes.Assets.InvalidFoundState, ErrorCategory.Validation, "قيمة نتيجة الفحص غير صالحة");

        var count = await context.AssetPhysicalCounts.FindAsync([request.CountId], ct);
        if (count is null)
            return Result<int>.Failure(ErrorCodes.Assets.CountNotFound, ErrorCategory.NotFound, "وثيقة الجرد غير موجودة");

        if (count.Status != CountStatus.InProgress)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidCountStatus, ErrorCategory.Validation, "لا يمكن تعديل سطور جرد بعد إكماله");

        var line = await context.AssetPhysicalCountDetails
            .FirstOrDefaultAsync(l => l.Id == request.LineId && l.AssetPhysicalCountId == request.CountId, ct);
        if (line is null)
            return Result<int>.Failure(ErrorCodes.Assets.CountLineNotFound, ErrorCategory.NotFound, "سطر الجرد غير موجود");

        if (!line.RowVersion.SequenceEqual(request.RowVersion))
            return Result<int>.Failure(ErrorCodes.Request.ConcurrencyConflict, ErrorCategory.Conflict, "تم تعديل السطر من مستخدم آخر، يرجى إعادة التحميل");

        line.IsFound = request.IsFound;
        line.PhysicalLocationId = request.PhysicalLocationId;
        line.PhysicalEmployeeId = request.PhysicalEmployeeId;
        line.PhysicalStatus = request.PhysicalStatus;
        line.DiscrepancyNotes = request.DiscrepancyNotes;
        line.LastModified = DateTimeOffset.UtcNow;

        count.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(line.Id);
    }
}

public class UpdateAssetPhysicalCountLineCommandValidator : AbstractValidator<UpdateAssetPhysicalCountLineCommand>
{
    public UpdateAssetPhysicalCountLineCommandValidator()
    {
        RuleFor(x => x.CountId)
            .GreaterThan(0).WithMessage("معرف وثيقة الجرد مطلوب");

        RuleFor(x => x.LineId)
            .GreaterThan(0).WithMessage("معرف سطر الجرد مطلوب");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("إصدار السطر مطلوب للتحقق من التزامن");
    }
}
