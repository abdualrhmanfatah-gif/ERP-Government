using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

public class DocumentStatusLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public int ChangedById { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? Reason { get; set; }
}
