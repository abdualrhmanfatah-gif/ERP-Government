using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.ItemUnits.Commands.UpdateItemUnit;

[Authorize(Policy = PermissionCodes.ItemsUpdate)]
public record UpdateItemUnitCommand(
    int Id,
    int ItemId,
    decimal ConversionFactor,
    bool IsBase) : IRequest<Result>;

public class UpdateItemUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateItemUnitCommand, Result>
{
    public async Task<Result> Handle(
        UpdateItemUnitCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ItemUnits.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Item unit not found."]);

        entity.ConversionFactor = request.ConversionFactor;

        if (request.IsBase && !entity.IsBase)
        {
            var existingBase = await context.ItemUnits
                .Where(iu => iu.ItemId == request.ItemId && iu.IsBase && iu.Id != request.Id)
                .ToListAsync(cancellationToken);
            foreach (var baseUnit in existingBase)
                baseUnit.IsBase = false;
        }

        entity.IsBase = request.IsBase;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateItemUnitCommandValidator : AbstractValidator<UpdateItemUnitCommand>
{
    public UpdateItemUnitCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid item unit.");

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("Invalid item.");

        RuleFor(x => x.ConversionFactor)
            .GreaterThan(0).WithMessage("Conversion factor must be greater than 0.");
    }
}
