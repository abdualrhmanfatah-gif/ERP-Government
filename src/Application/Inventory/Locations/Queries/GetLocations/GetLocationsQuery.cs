using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Locations.Queries.GetLocations;

[Authorize(Policy = PermissionCodes.LocationsView)]
public record GetLocationsQuery(
    string? Search = null,
    bool? IsActive = null,
    int? ParentLocationId = null) : IRequest<IReadOnlyList<Location>>;

public class GetLocationsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetLocationsQuery, IReadOnlyList<Location>>
{
    public async Task<IReadOnlyList<Location>> Handle(
        GetLocationsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Location> query = context.Locations
            .Include(l => l.ParentLocation);

        if (request.IsActive.HasValue)
            query = query.Where(l => l.IsActive == request.IsActive.Value);

        if (request.ParentLocationId.HasValue)
            query = query.Where(l => l.ParentLocationId == request.ParentLocationId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(l =>
                l.Code.ToLower().Contains(search) ||
                l.Name.ToLower().Contains(search) ||
                (l.City != null && l.City.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(l => l.Level ?? -1)
            .ThenBy(l => l.Code)
            .ToListAsync(cancellationToken);
    }
}
