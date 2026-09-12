using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Warehouses.Commands.ToggleWarehouseActive;

[Authorize(Policy = PermissionCodes.WarehousesUpdate)]
public record ToggleWarehouseActiveCommand(int Id) : IRequest<Result>;

public class ToggleWarehouseActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleWarehouseActiveCommand, Result>
{
    public async Task<Result> Handle(
        ToggleWarehouseActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Warehouses.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Warehouse not found."]);

        entity.IsActive = !entity.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
