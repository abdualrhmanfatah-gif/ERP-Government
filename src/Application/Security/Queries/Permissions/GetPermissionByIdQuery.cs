using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Security.Queries.Permissions;

// Q-S004 — GetPermissionByIdQuery
public class GetPermissionByIdQuery : IRequest<SecurityPermissionDto?>
{
    public int Id { get; init; }
}

public class GetPermissionByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetPermissionByIdQuery, SecurityPermissionDto?>
{
    public async Task<SecurityPermissionDto?> Handle(
        GetPermissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.SecurityPermissions
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<SecurityPermissionDto>(entity);
    }
}
