using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class AuditTrail : BaseLongEntity, IImmutableEntity
{
    public string EventCategory { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public int? DocumentId { get; set; }
    public AuditAction Action { get; set; }
    public int? UserId { get; set; }
    public bool Success { get; set; } = true;
    public string? FailureReason { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? SessionId { get; set; }
    public string? ChangeSummary { get; set; }
    public string? FieldChanges { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public User? User { get; set; }
}
