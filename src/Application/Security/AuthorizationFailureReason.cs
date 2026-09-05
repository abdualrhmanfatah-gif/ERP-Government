namespace ERP_Government.Application.Security;

/// <summary>
/// Categorizes why authorization failed for audit and diagnostic purposes.
/// </summary>
public enum AuthorizationFailureReason
{
    /// <summary>No valid authentication token or session.</summary>
    Unauthenticated,

    /// <summary>Authenticated but missing the required permission for the endpoint.</summary>
    Unauthorized,

    /// <summary>Permission lookup service failed (fail-closed behavior).</summary>
    ServiceError
}
