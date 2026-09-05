using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.CostCenters;

// Q-O005 — GetCostCentersQuery
[Authorize(Policy = PermissionCodes.CostCentersView)]
public class GetCostCentersQuery : IRequest<List<CostCenterDto>>
{
    public bool? IsActive { get; init; }
}

public class GetCostCentersQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetCostCentersQuery, List<CostCenterDto>>
{
    public async Task<List<CostCenterDto>> Handle(
        GetCostCentersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.CostCenters
            .Include(x => x.OrganizationUnit)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<CostCenterDto>>(items);
    }
}
