using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Domain.FinancialSettings.Entities;

public class YearEndClosingEntry : BaseAuditableEntity
{
    public string ClosingEntryNumber { get; set; } = string.Empty;
    public int FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;
    public DateOnly ClosingDate { get; set; }
    public string? Description { get; set; }
    public ClosingEntryStatus Status { get; set; }
    public bool IsReversal { get; set; }
    public int? ReversalOfId { get; set; }
    public YearEndClosingEntry? ReversalOf { get; set; }
    public int? JournalEntryId { get; set; }
    public string? ApprovedById { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
