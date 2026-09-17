using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Queries.Users;

// T023 — GetUserPermissionsQuery (effective permissions)
[Authorize(Policy = PermissionCodes.UsersView)]
public class GetUserPermissionsQuery : IRequest<List<EffectivePermissionDto>>
{
    public int UserId { get; init; }
}

public class GetUserPermissionsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetUserPermissionsQuery, List<EffectivePermissionDto>>
{
    public async Task<List<EffectivePermissionDto>> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        // Get role-inherited permissions
        var rolePermissions = await context.Users
            .Where(u => u.Id == request.UserId)
            .Join(context.SecurityRoles,
                u => u.RoleId,
                r => r.Id,
                (u, r) => r)
            .Where(r => r.IsActive)
            .Join(context.RolePermissions,
                r => r.Id,
                rp => rp.RoleId,
                (r, rp) => rp)
            .Join(context.SecurityPermissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p)
            .Where(p => p.IsActive)
            .Select(p => new EffectivePermissionDto
            {
                PermissionId = p.Id,
                Code = p.Code,
                Name = p.Name,
                Source = "role",
                IsGranted = true,
                Reason = null
            })
            .ToListAsync(cancellationToken);

        var effectiveMap = new Dictionary<int, EffectivePermissionDto>();
        foreach (var rp in rolePermissions)
            effectiveMap[rp.PermissionId] = rp;

        // Apply user-level grants (IsGranted=true)
        var grants = await context.UserPermissions
            .Where(up => up.UserId == request.UserId
                && up.IsGranted
                && (up.EffectiveFrom == null || up.EffectiveFrom <= now)
                && (up.EffectiveTo == null || up.EffectiveTo >= now))
            .Join(context.SecurityPermissions,
                up => up.PermissionId,
                p => p.Id,
                (up, p) => new { up, p })
            .Where(x => x.p.IsActive)
            .Select(x => new EffectivePermissionDto
            {
                PermissionId = x.p.Id,
                Code = x.p.Code,
                Name = x.p.Name,
                Source = "override",
                IsGranted = true,
                Reason = x.up.Reason
            })
            .ToListAsync(cancellationToken);

        foreach (var g in grants)
            effectiveMap[g.PermissionId] = g;

        // Apply user-level revokes (IsGranted=false)
        var revokes = await context.UserPermissions
            .Where(up => up.UserId == request.UserId
                && !up.IsGranted
                && (up.EffectiveFrom == null || up.EffectiveFrom <= now)
                && (up.EffectiveTo == null || up.EffectiveTo >= now))
            .Join(context.SecurityPermissions,
                up => up.PermissionId,
                p => p.Id,
                (up, p) => new { up, p })
            .Where(x => x.p.IsActive)
            .Select(x => new EffectivePermissionDto
            {
                PermissionId = x.p.Id,
                Code = x.p.Code,
                Name = x.p.Name,
                Source = "override",
                IsGranted = false,
                Reason = x.up.Reason
            })
            .ToListAsync(cancellationToken);

        foreach (var r in revokes)
            effectiveMap[r.PermissionId] = r;

        return effectiveMap.Values
            .OrderBy(x => x.Code)
            .ToList();
    }
}
