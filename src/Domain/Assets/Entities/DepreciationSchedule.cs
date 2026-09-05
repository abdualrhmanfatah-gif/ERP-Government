using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class DepreciationSchedule : BaseAuditableEntity
{
    public int AssetId { get; set; }
    public DateOnly DepreciationDate { get; set; }
    public int? FiscalYearId { get; set; }
    public int? FiscalPeriodId { get; set; }
    public string? DepreciationMethod { get; set; }
    public decimal? DepreciationBase { get; set; }
    public decimal? DepreciationRate { get; set; }
    public int? PeriodNumber { get; set; }
    public int? TotalPeriods { get; set; }
    public decimal Amount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? JournalEntryId { get; set; }
    public bool IsReversed { get; set; }
    public int? ReversalOfId { get; set; }
    public string? ReversalReason { get; set; }
    public DateOnly? ReversalDate { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Asset? Asset { get; set; }
    public DepreciationSchedule? ReversalOf { get; set; }
}
