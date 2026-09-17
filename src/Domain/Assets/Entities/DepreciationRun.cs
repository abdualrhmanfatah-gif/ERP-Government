using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Domain.Assets.Entities;

public class DepreciationRun : BaseAuditableEntity
{
    public string RunNumber { get; set; } = string.Empty;
    public int FiscalYearId { get; set; }
    public int FiscalPeriodId { get; set; }
    public DateOnly DepreciationDate { get; set; }
    public string MissedPeriodsPolicy { get; set; } = string.Empty;
    public DepreciationRunStatus Status { get; set; }
    public decimal TotalDepreciation { get; set; }
    public string? Notes { get; set; }
    public int? JournalEntryId { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public FiscalYear? FiscalYear { get; set; }
    public FiscalPeriod? FiscalPeriod { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public ICollection<DepreciationScheduleLine> ScheduleLines { get; set; } = new List<DepreciationScheduleLine>();
}
