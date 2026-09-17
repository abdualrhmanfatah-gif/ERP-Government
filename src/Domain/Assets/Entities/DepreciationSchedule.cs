using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class DepreciationSchedule : BaseAuditableEntity
{
    public int DepreciationRunId { get; set; }
    public int AssetId { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public int? PeriodNumber { get; set; }
    public int? TotalPeriods { get; set; }
    public decimal DepreciationBase { get; set; }
    public decimal ResidualValue { get; set; }
    public decimal OpeningBookValue { get; set; }
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal ClosingAccumulatedDepreciation { get; set; }
    public decimal ClosingBookValue { get; set; }

    public AssetDepreciationRun? DepreciationRun { get; set; }
    public Asset? Asset { get; set; }
}
