using ERP_Government.Domain.Common;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Domain.Procurement.Entities;

public class SupplierInvoice : BaseAuditableEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public int PurchaseOrderId { get; set; }
    public int SupplierPartyId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal? GrandTotal { get; set; }
    public DateOnly? DueDate { get; set; }
    public SupplierInvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public ICollection<SupplierInvoiceDetail> Details { get; set; } = [];
}
