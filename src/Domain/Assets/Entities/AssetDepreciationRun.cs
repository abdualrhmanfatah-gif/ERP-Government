using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Entities;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetDepreciationRun : BaseAuditableEntity
{
    public string RunNumber { get; set; } = string.Empty;
    public int FiscalYearId { get; set; }
    public int FiscalPeriodId { get; set; }
    public DateOnly DepreciationDate { get; set; }
    public AssetDepreciationRunStatus Status { get; set; } = AssetDepreciationRunStatus.Draft;
    public int? JournalEntryId { get; set; }
    public decimal TotalDepreciation { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedBy { get; set; }
    public int? ReversalJournalEntryId { get; set; }
    public DateOnly? ReversalDate { get; set; }
    public DateTimeOffset? ReversedAt { get; set; }
    public string? ReversedBy { get; set; }
    public string? ReversalReason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public FiscalYear? FiscalYear { get; set; }
    public FiscalPeriod? FiscalPeriod { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public JournalEntry? ReversalJournalEntry { get; set; }
    public ICollection<DepreciationSchedule> Schedules { get; set; } = new List<DepreciationSchedule>();
}
