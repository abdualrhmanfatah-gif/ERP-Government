using ERP_Government.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security;

/// <summary>
/// Resolves effective permissions for a user via the union-minus formula:
/// RolePermissions(linked Role) ∪ UserPermissions(IsGranted=true) \ UserPermissions(IsGranted=false)
/// </summary>
public sealed class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _dbContext;

    public PermissionService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Get role-inherited permissions (where Role.IsActive and EffectiveFrom/To permit)
        var rolePermissions = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Join(_dbContext.SecurityRoles,
                u => u.RoleId,
                r => r.Id,
                (u, r) => r)
            .Where(r => r.IsActive)
            .Join(_dbContext.RolePermissions,
                r => r.Id,
                rp => rp.RoleId,
                (r, rp) => rp)
            .Join(_dbContext.SecurityPermissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p)
            .Where(p => p.IsActive)
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var effectiveSet = new HashSet<string>(rolePermissions);

        // Apply user-level grants (IsGranted=true)
        var grants = await _dbContext.UserPermissions
            .Where(up => up.UserId == userId
                && up.IsGranted
                && (up.EffectiveFrom == null || up.EffectiveFrom <= now)
                && (up.EffectiveTo == null || up.EffectiveTo >= now))
            .Join(_dbContext.SecurityPermissions,
                up => up.PermissionId,
                p => p.Id,
                (up, p) => p)
            .Where(p => p.IsActive)
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        foreach (var code in grants)
            effectiveSet.Add(code);

        // Apply user-level revokes (IsGranted=false)
        var revokes = await _dbContext.UserPermissions
            .Where(up => up.UserId == userId
                && !up.IsGranted
                && (up.EffectiveFrom == null || up.EffectiveFrom <= now)
                && (up.EffectiveTo == null || up.EffectiveTo >= now))
            .Join(_dbContext.SecurityPermissions,
                up => up.PermissionId,
                p => p.Id,
                (up, p) => p)
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        foreach (var code in revokes)
            effectiveSet.Remove(code);

        return effectiveSet;
    }
}
