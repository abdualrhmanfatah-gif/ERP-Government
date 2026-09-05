using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class StockTakeDetail : BaseAuditableEntity
{
    public int StockTakeId { get; set; }
    public int ItemId { get; set; }
    public int UnitId { get; set; }
    public decimal? ConversionFactor { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal QuantityDifference { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? DifferenceAmount { get; set; }
    public int? LotId { get; set; }
    public int? BinId { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public StockTake? StockTake { get; set; }
    public Item? Item { get; set; }
    public Unit? Unit { get; set; }
}
