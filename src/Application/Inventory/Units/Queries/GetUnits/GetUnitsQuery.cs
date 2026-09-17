using Unit = ERP_Government.Domain.Inventory.Entities.Unit;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Units.Queries.GetUnits;

[Authorize(Policy = PermissionCodes.UnitsView)]
public record GetUnitsQuery(
    string? Search = null,
    bool? IsActive = null) : IRequest<IReadOnlyList<Unit>>;

public class GetUnitsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetUnitsQuery, IReadOnlyList<Unit>>
{
    public async Task<IReadOnlyList<Unit>> Handle(
        GetUnitsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Unit> query = context.Units
            .Include(u => u.BaseUnit);

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Code.ToLower().Contains(search) ||
                u.Name.ToLower().Contains(search));
        }

        return await query
            .OrderBy(u => u.Code)
            .ToListAsync(cancellationToken);
    }
}
