using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class StockTake : BaseAuditableEntity
{
    public string StockTakeNumber { get; set; } = string.Empty;
    public DateOnly StockTakeDate { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string StockTakeType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CountedById { get; set; }
    public int? ReviewedById { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Warehouse? Warehouse { get; set; }
    public Location? Location { get; set; }
}
