using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Security.Queries.Permissions;

// Q-S003 — GetPermissionsQuery
public class GetPermissionsQuery : IRequest<List<SecurityPermissionDto>>
{
    public string? Module { get; init; }
    public bool? IsActive { get; init; }
}

public class GetPermissionsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetPermissionsQuery, List<SecurityPermissionDto>>
{
    public async Task<List<SecurityPermissionDto>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.SecurityPermissions.AsQueryable();

        if (!string.IsNullOrEmpty(request.Module))
            query = query.Where(x => x.Module == request.Module);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Action)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<SecurityPermissionDto>>(items);
    }
}
