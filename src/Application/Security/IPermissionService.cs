namespace ERP_Government.Application.Security;

/// <summary>
/// Resolves fine-grained permissions for a user.
/// Used by AuthorizationBehaviour to check endpoint-level permissions.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets all permission codes granted to the specified user via their roles.
    /// </summary>
    Task<IReadOnlySet<string>> GetPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}
