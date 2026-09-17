using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using FluentValidation;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Assets.Commands.DeactivateAsset;

[Authorize(Policy = PermissionCodes.AssetsUpdate)]
public record DeactivateAssetCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class DeactivateAssetCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateAssetCommand, Result>
{
    private static readonly HashSet<string> AllowedStatuses = ["Active", "UnderMaintenance"];

    public async Task<Result> Handle(DeactivateAssetCommand request, CancellationToken ct)
    {
        var entity = await context.Assets.FindAsync([request.Id], ct);
        if (entity is null)
            return Result.Failure(
                ErrorCodes.Assets.AssetNotFound,
                ErrorCategory.NotFound,
                $"الأصل بالمعرف {request.Id} غير موجود");

        entity.RowVersion = request.RowVersion;

        if (!AllowedStatuses.Contains(entity.Status))
            return Result.Failure(
                ErrorCodes.Assets.InvalidStatusTransition,
                ErrorCategory.Validation,
                $"لا يمكن تعطيل الأصل من حالة {entity.Status}");

        entity.Status = "Disposed";
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeactivateAssetCommandValidator : AbstractValidator<DeactivateAssetCommand>
{
    public DeactivateAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف الأصل غير صالح");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("إصدار السطر مطلوب للتحقق من التزامن");
    }
}
