using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class GoodsReceiptNote : BaseAuditableEntity
{
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime GRNDate { get; set; }
    public int? SupplierId { get; set; }
    public int PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public int WarehouseId { get; set; }
    public int LocationId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public decimal? TotalQuantity { get; set; }
    public decimal? TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
