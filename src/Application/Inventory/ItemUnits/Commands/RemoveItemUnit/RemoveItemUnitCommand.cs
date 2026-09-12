using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.ItemUnits.Commands.RemoveItemUnit;

[Authorize(Policy = PermissionCodes.ItemsUpdate)]
public record RemoveItemUnitCommand(int Id, int ItemId) : IRequest<Result>;

public class RemoveItemUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RemoveItemUnitCommand, Result>
{
    public async Task<Result> Handle(
        RemoveItemUnitCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ItemUnits.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Item unit not found."]);

        if (entity.IsBase)
        {
            var otherBaseExists = await context.ItemUnits
                .AnyAsync(iu => iu.ItemId == request.ItemId && iu.IsBase && iu.Id != request.Id, cancellationToken);
            if (!otherBaseExists)
                return Result.Failure(["Cannot remove the only base unit. Designate another unit as base first."]);
        }

        context.ItemUnits.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
