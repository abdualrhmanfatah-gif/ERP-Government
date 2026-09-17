using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class DepreciationScheduleLine : BaseAuditableEntity
{
    public int DepreciationRunId { get; set; }
    public int AssetId { get; set; }
    public int FiscalYearId { get; set; }
    public int FiscalPeriodId { get; set; }
    public DateOnly DepreciationDate { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public int PeriodNumber { get; set; }
    public int? TotalPeriods { get; set; }
    public decimal DepreciationBase { get; set; }
    public decimal ResidualValue { get; set; }
    public decimal OpeningBookValue { get; set; }
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal Amount { get; set; }
    public decimal ClosingAccumulatedDepreciation { get; set; }
    public decimal ClosingBookValue { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public DepreciationRun? DepreciationRun { get; set; }
    public Asset? Asset { get; set; }
}
