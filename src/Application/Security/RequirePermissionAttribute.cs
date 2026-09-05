namespace ERP_Government.Application.Security;

/// <summary>
/// Declares the permission code required to access an endpoint.
/// Applied to Minimal API endpoint methods for startup validation and documentation.
/// Complements ASP.NET Core's RequireAuthorization() by providing metadata for startup scanning.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RequirePermissionAttribute : Attribute
{
    /// <summary>
    /// The permission code required (e.g., "Accounting.Journals.Read").
    /// Must match a value from <see cref="Common.Security.PermissionCodes"/>.
    /// </summary>
    public string PermissionCode { get; }

    public RequirePermissionAttribute(string permissionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);
        PermissionCode = permissionCode;
    }
}
