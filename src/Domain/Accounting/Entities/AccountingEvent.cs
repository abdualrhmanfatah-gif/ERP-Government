using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Domain.Accounting.Entities;

public class AccountingEvent : BaseAuditableEntity
{
    public EventType EventType { get; set; }
    public string SourceDocumentType { get; set; } = string.Empty;
    public int SourceDocumentId { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public int? JournalEntryId { get; set; }
    public EventCategory EventCategory { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public byte[] RowVersion { get; set; } = [];

    // Same-module FK (set after processing)
    public JournalEntry? JournalEntry { get; set; }
}
