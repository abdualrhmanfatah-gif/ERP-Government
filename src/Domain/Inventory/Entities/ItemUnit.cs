using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class ItemUnit : BaseAuditableEntity
{
    public int ItemId { get; set; }
    public int UnitId { get; set; }
    public decimal ConversionFactor { get; set; }
    public bool IsBase { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Item? Item { get; set; }
    public Unit? Unit { get; set; }
}
