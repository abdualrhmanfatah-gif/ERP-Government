using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

public class SecurityAuditLog : BaseLongEntity, IImmutableEntity
{
    public string EventCategory { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? EntityName { get; set; }
    public int? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
    public string? SessionId { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public User User { get; set; } = null!;
}
