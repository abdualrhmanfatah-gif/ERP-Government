using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class Notification : BaseLongAuditableEntity
{
    public int UserId { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public int? DocumentId { get; set; }
    public NotificationPriority Priority { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public User User { get; set; } = null!;
}
