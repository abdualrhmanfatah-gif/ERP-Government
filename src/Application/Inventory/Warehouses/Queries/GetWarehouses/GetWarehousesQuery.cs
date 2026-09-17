using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Warehouses.Queries.GetWarehouses;

[Authorize(Policy = PermissionCodes.WarehousesView)]
public record GetWarehousesQuery(
    string? Search = null,
    bool? IsActive = null) : IRequest<IReadOnlyList<Warehouse>>;

public class GetWarehousesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetWarehousesQuery, IReadOnlyList<Warehouse>>
{
    public async Task<IReadOnlyList<Warehouse>> Handle(
        GetWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Warehouse> query = context.Warehouses
            .Include(w => w.Location);

        if (request.IsActive.HasValue)
            query = query.Where(w => w.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(w =>
                w.Code.ToLower().Contains(search) ||
                w.Name.ToLower().Contains(search));
        }

        return await query
            .OrderBy(w => w.Code)
            .ToListAsync(cancellationToken);
    }
}
