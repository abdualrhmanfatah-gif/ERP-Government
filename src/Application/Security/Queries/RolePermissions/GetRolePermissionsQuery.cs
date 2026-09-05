using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Queries.RolePermissions;

// FEATURE-008 — GetRolePermissionsQuery
public class GetRolePermissionsQuery : IRequest<List<RolePermissionDto>>
{
    public int RoleId { get; init; }
}

public class GetRolePermissionsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetRolePermissionsQuery, List<RolePermissionDto>>
{
    public async Task<List<RolePermissionDto>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.RolePermissions
            .Where(rp => rp.RoleId == request.RoleId)
            .Select(rp => new RolePermissionDto
            {
                RoleId = rp.RoleId,
                PermissionId = rp.PermissionId,
                PermissionCode = rp.Permission.Code,
                PermissionName = rp.Permission.Name
            })
            .OrderBy(x => x.PermissionCode)
            .ToListAsync(cancellationToken);
    }
}
