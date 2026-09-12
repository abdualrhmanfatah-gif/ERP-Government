using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.ItemUnits.Commands.AddItemUnit;

[Authorize(Policy = PermissionCodes.ItemsUpdate)]
public record AddItemUnitCommand(
    int ItemId,
    int UnitId,
    decimal ConversionFactor,
    bool IsBase = false) : IRequest<Result<int>>;

public class AddItemUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<AddItemUnitCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        AddItemUnitCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.ItemUnits
            .AnyAsync(iu => iu.ItemId == request.ItemId && iu.UnitId == request.UnitId, cancellationToken);
        if (exists)
            return Result<int>.Failure(["This unit is already linked to this item."]);

        if (request.IsBase)
        {
            var existingBase = await context.ItemUnits
                .Where(iu => iu.ItemId == request.ItemId && iu.IsBase)
                .ToListAsync(cancellationToken);
            foreach (var baseUnit in existingBase)
                baseUnit.IsBase = false;
        }

        var entity = new ItemUnit
        {
            ItemId = request.ItemId,
            UnitId = request.UnitId,
            ConversionFactor = request.ConversionFactor,
            IsBase = request.IsBase,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.ItemUnits.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class AddItemUnitCommandValidator : AbstractValidator<AddItemUnitCommand>
{
    public AddItemUnitCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("Invalid item.");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("Invalid unit.");

        RuleFor(x => x.ConversionFactor)
            .GreaterThan(0).WithMessage("Conversion factor must be greater than 0.");
    }
}
