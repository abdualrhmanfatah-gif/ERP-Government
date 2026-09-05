using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetDisposal : BaseAuditableEntity
{
    public string DisposalNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public DateOnly DisposalDate { get; set; }
    public string DisposalMethod { get; set; } = string.Empty;
    public decimal BookValueAtDisposal { get; set; }
    public decimal AccumulatedDepreciationAtDisposal { get; set; }
    public decimal? SaleProceeds { get; set; }
    public decimal? DisposalCost { get; set; }
    public decimal? NetProceeds { get; set; }
    public decimal? GainOrLoss { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerContact { get; set; }
    public string? CurrencyCode { get; set; }
    public int? AccountDisposalId { get; set; }
    public int? JournalEntryId { get; set; }
    public bool IsPosted { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Asset? Asset { get; set; }
}
