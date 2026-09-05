using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class Item : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int UnitId { get; set; }
    public int? SupplierId { get; set; }
    public string? Barcode { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public decimal? OpeningStock { get; set; }
    public decimal? AvailableQuantity { get; set; }
    public decimal? ReservedQuantity { get; set; }
    public decimal? AverageCost { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public int? LeadTimeDays { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public ItemCategory? Category { get; set; }
    public Unit? Unit { get; set; }
}
