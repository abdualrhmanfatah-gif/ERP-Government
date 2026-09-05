using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Audit trail of each recurring entry execution attempt.
/// One row per execution — tracks JournalEntry generation, status, and error details.
/// </summary>
public class RecurringEntryExecutionLog : BaseAuditableEntity
{
    public int RecurringEntryId { get; set; }
    public DateOnly ExecutionDate { get; set; }
    public int? GeneratedJournalEntryId { get; set; }
    public RecurringEntryExecutionStatus Status { get; set; } = RecurringEntryExecutionStatus.Created;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string TriggeredBy { get; set; } = "Scheduler";
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public RecurringEntry RecurringEntry { get; set; } = null!;
    public JournalEntry? GeneratedJournalEntry { get; set; }
}
