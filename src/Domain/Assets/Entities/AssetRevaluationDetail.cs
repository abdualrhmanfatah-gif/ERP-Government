using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetRevaluationDetail : BaseEntity
{
    public int AssetTransactionId { get; set; }
    public string? RevaluationMethod { get; set; }
    public decimal OldBookValue { get; set; }
    public decimal NewBookValue { get; set; }
    public decimal? RevaluationAmount { get; set; }
    public string? RevaluationType { get; set; }
    public string? AppraiserName { get; set; }
    public string? ReportNumber { get; set; }

    public AssetTransaction? AssetTransaction { get; set; }
}
