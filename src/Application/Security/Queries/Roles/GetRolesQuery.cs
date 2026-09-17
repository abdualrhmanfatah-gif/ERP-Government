using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Security.Queries.Roles;

// Q-S001 — GetRolesQuery
[Authorize(Policy = PermissionCodes.RolesView)]
public class GetRolesQuery : IRequest<List<SecurityRoleDto>>
{
    public bool? IsActive { get; init; }
}

public class GetRolesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetRolesQuery, List<SecurityRoleDto>>
{
    public async Task<List<SecurityRoleDto>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.SecurityRoles.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<SecurityRoleDto>>(items);
    }
}
