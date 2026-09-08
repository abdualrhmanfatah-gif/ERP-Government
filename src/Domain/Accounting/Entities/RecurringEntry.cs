using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Recurring entries generated automatically on schedule per schema table 45.
/// </summary>
public class RecurringEntry : BaseAuditableEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public int JournalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RecurringFrequency Frequency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly NextExecutionDate { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public decimal? Amount { get; set; }
    public int? CurrencyId { get; set; }
    public int? FundId { get; set; } // Deferred FK to Module 5 — Funds table not yet implemented
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public string? DescriptionTemplate { get; set; }
    public RecurringEntryStatus Status { get; set; } = RecurringEntryStatus.Active;
    public int? GeneratedJournalEntryId { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    // Same-module FKs
    public JournalEntryTemplate? Template { get; set; }
    public Journal Journal { get; set; } = null!;
    public JournalEntry? GeneratedJournalEntry { get; set; }

    // Cross-module FKs (Module 3)
    public CostCenter? CostCenter { get; set; }
    public Project? Project { get; set; }

    // Cross-module FKs (Module 1)
    // public Currency? Currency { get; set; }
}
