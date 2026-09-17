using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetDisposalDetail : BaseEntity
{
    public int AssetTransactionId { get; set; }
    public string DisposalMethod { get; set; } = string.Empty;
    public decimal BookValueAtDisposal { get; set; }
    public decimal AccumulatedDepreciationAtDisposal { get; set; }
    public decimal? SaleProceeds { get; set; }
    public decimal? DisposalCost { get; set; }
    public decimal? NetProceeds { get; set; }
    public decimal? GainOrLoss { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerContact { get; set; }

    public AssetTransaction? AssetTransaction { get; set; }
}
