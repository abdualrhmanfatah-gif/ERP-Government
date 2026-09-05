using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class StockTransaction : BaseAuditableEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public int WarehouseId { get; set; }
    public int LocationId { get; set; }
    public int? BinId { get; set; }
    public int ItemId { get; set; }
    public int? LotId { get; set; }
    public int UnitId { get; set; }
    public decimal? ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? QuantityBefore { get; set; }
    public decimal? QuantityAfter { get; set; }
    public string? Notes { get; set; }
    public bool IsReversed { get; set; }
    public int? ReversalOfId { get; set; }
    public string? ReversalReason { get; set; }
    public DateTime? ReversalDate { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Item? Item { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Location? Location { get; set; }
    public Unit? Unit { get; set; }
    public StockTransaction? ReversalOf { get; set; }
}
