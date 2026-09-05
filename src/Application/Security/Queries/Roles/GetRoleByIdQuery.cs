using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Security.Queries.Roles;

// Q-S002 — GetRoleByIdQuery
public class GetRoleByIdQuery : IRequest<SecurityRoleDto?>
{
    public int Id { get; init; }
}

public class GetRoleByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetRoleByIdQuery, SecurityRoleDto?>
{
    public async Task<SecurityRoleDto?> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.SecurityRoles
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<SecurityRoleDto>(entity);
    }
}
