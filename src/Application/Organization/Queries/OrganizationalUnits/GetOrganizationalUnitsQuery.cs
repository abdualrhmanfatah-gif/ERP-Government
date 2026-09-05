using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.OrganizationalUnits;

// Q-O001 — GetOrganizationalUnitsQuery
[Authorize(Policy = PermissionCodes.OrgUnitsView)]
public class GetOrganizationalUnitsQuery : IRequest<List<OrganizationalUnitDto>>
{
    public bool? IsActive { get; init; }
}

public class GetOrganizationalUnitsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetOrganizationalUnitsQuery, List<OrganizationalUnitDto>>
{
    public async Task<List<OrganizationalUnitDto>> Handle(
        GetOrganizationalUnitsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.OrganizationalUnits.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderBy(x => x.ParentPath)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<OrganizationalUnitDto>>(items);
    }
}
