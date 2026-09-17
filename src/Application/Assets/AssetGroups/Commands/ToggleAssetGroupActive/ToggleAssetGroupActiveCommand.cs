using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using FluentValidation;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetGroups.Commands.ToggleAssetGroupActive;

[Authorize]
public record ToggleAssetGroupActiveCommand(
    int Id,
    bool IsActive,
    byte[] RowVersion) : IRequest<Result<int>>;

public class ToggleAssetGroupActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleAssetGroupActiveCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ToggleAssetGroupActiveCommand request, CancellationToken ct)
    {
        var entity = await context.AssetGroups.FindAsync([request.Id], ct);
        if (entity is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetGroupNotFound, ErrorCategory.NotFound, "المجموعة غير موجودة");

        if (entity.IsActive == request.IsActive)
        {
            var message = request.IsActive ? "المجموعة مفعلة بالفعل" : "المجموعة معطلة بالفعل";
            return Result<int>.Failure(ErrorCodes.Assets.AlreadyActive, ErrorCategory.Validation, message);
        }

        if (!request.IsActive && await context.Assets.AnyAsync(a => a.AssetGroupId == request.Id, ct))
            return Result<int>.Failure(ErrorCodes.Assets.DeactivationBlocked, ErrorCategory.Validation, "لا يمكن تعطيل مجموعة مرتبطة بأصول");

        entity.IsActive = request.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Failure(ErrorCodes.Request.ConcurrencyConflict, ErrorCategory.Conflict, "تم تعديل المجموعة من مستخدم آخر");
        }

        return Result<int>.Success(entity.Id);
    }
}

public class ToggleAssetGroupActiveCommandValidator : AbstractValidator<ToggleAssetGroupActiveCommand>
{
    public ToggleAssetGroupActiveCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion مطلوب");
    }
}
