using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.OrganizationalUnits;

// Q-O003 — GetOrganizationalUnitByIdQuery
[Authorize(Policy = PermissionCodes.OrgUnitsView)]
public class GetOrganizationalUnitByIdQuery : IRequest<OrganizationalUnitDto>
{
    public int Id { get; init; }
}

public class GetOrganizationalUnitByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetOrganizationalUnitByIdQuery, OrganizationalUnitDto>
{
    public async Task<OrganizationalUnitDto> Handle(
        GetOrganizationalUnitByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.OrganizationalUnits
            .FindAsync(request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(OrganizationalUnit), request.Id)
            : mapper.Map<OrganizationalUnitDto>(entity);
    }
}
