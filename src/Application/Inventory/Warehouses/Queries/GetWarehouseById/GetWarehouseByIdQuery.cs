using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Warehouses.Queries.GetWarehouseById;

[Authorize(Policy = PermissionCodes.WarehousesView)]
public record GetWarehouseByIdQuery(int Id) : IRequest<Result<Warehouse>>;

public class GetWarehouseByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetWarehouseByIdQuery, Result<Warehouse>>
{
    public async Task<Result<Warehouse>> Handle(
        GetWarehouseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Warehouses
            .Include(w => w.Location)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<Warehouse>.Failure(ErrorCodes.Inventory.WarehouseNotFound, ErrorCategory.NotFound, $"Warehouse with ID {request.Id} not found.");

        return Result<Warehouse>.Success(entity);
    }
}
