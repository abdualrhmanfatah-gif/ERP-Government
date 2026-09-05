namespace ERP_Government.Application.Security;

/// <summary>
/// Structured audit event emitted on every authorization failure (401/403).
/// Used for security monitoring, incident response, and compliance logging.
/// </summary>
public sealed record AuthorizationAuditEvent
{
    public int? UserId { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string? PermissionRequired { get; init; }
    public AuthorizationFailureReason Reason { get; init; }
    public string? FailureDetail { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string? IpAddress { get; init; }
}
