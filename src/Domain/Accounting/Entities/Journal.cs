using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Journals for classifying and posting accounting entries per schema table 37.
/// </summary>
public class Journal : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public JournalType Type { get; set; }
    public int? AccountId { get; set; }
    public int? SuspenseAccountId { get; set; }
    public bool AllowForeignCurrency { get; set; }
    public int? SequenceId { get; set; }
    public bool RequireApprovalBeforePosting { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Account? Account { get; set; }
    public Account? SuspenseAccount { get; set; }
    // Deferred FK to Module 1 — DocumentSequences table already implemented
    // public DocumentSequence? Sequence { get; set; }
}
