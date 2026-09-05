using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Domain.Accounting.Entities;

public class JournalEntry : BaseAuditableEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public string? Ref { get; set; }
    public DateOnly DocumentDate { get; set; }
    public DateOnly? PostingDate { get; set; }
    public MoveEntryType? EntryType { get; set; }
    public EntryStatus EntryStatus { get; set; } = EntryStatus.Draft;
    public int? JournalId { get; set; }
    public int PeriodId { get; set; }
    public int FiscalYearId { get; set; }
    public string? Narration { get; set; }
    public int? SourceEventId { get; set; }
    public int? ReversalOfId { get; set; }
    public string? ReversalReason { get; set; }
    public int? PostedById { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public int? CancelledById { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public bool IsSystemGenerated { get; set; }
    public byte[] RowVersion { get; set; } = [];

    // Self-referential
    public JournalEntry? ReversalOf { get; set; }

    // Same-module FKs
    public Journal? Journal { get; set; }
    public AccountingEvent? SourceEvent { get; set; }

    // Cross-module FKs (Module 1)
    public FiscalPeriod? Period { get; set; }
    public FiscalYear? FiscalYear { get; set; }

    // User navigation for audit metadata
    public User? PostedBy { get; set; }
    public User? CancelledBy { get; set; }
}
