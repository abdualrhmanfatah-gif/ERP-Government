using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class SupplierInvoiceDetail : BaseAuditableEntity
{
    public int SupplierInvoiceId { get; set; }
    public int PurchaseOrderDetailId { get; set; }
    public int? GoodsReceiptNoteDetailId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineTotal { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public SupplierInvoice SupplierInvoice { get; set; } = null!;
    public PurchaseOrderDetail PurchaseOrderDetail { get; set; } = null!;
}
