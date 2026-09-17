using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.OrganizationalUnits;

// Q-O003 — GetOrganizationalUnitByIdQuery
[Authorize(Policy = PermissionCodes.OrgUnitsView)]
public class GetOrganizationalUnitByIdQuery : IRequest<Result<OrganizationalUnitDto>>
{
    public int Id { get; init; }
}

public class GetOrganizationalUnitByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetOrganizationalUnitByIdQuery, Result<OrganizationalUnitDto>>
{
    public async Task<Result<OrganizationalUnitDto>> Handle(
        GetOrganizationalUnitByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.OrganizationalUnits
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<OrganizationalUnitDto>.Failure(ErrorCodes.Organization.OrgUnitNotFound, ErrorCategory.NotFound, $"Organizational unit with ID {request.Id} not found.");

        return Result<OrganizationalUnitDto>.Success(mapper.Map<OrganizationalUnitDto>(entity));
    }
}
