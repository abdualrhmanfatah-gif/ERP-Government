using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.Warehouses.Queries.GetWarehouseById;

public record GetWarehouseByIdQuery(int Id) : IRequest<Warehouse?>;

public class GetWarehouseByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetWarehouseByIdQuery, Warehouse?>
{
    public async Task<Warehouse?> Handle(
        GetWarehouseByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Warehouses
            .Include(w => w.Location)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
    }
}
