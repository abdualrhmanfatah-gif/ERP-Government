using ERP_Government.Domain.Common;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Domain.Procurement.Entities;

public class PurchaseOrder : BaseAuditableEntity
{
    public string PONumber { get; set; } = string.Empty;
    public DateTime PODate { get; set; }
    public int? PurchaseRequestId { get; set; }
    public int? QuotationId { get; set; }
    public int SupplierPartyId { get; set; }
    public int? WarehouseId { get; set; }
    public int? DeliveryLocationId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal? GrandTotal { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PurchaseRequest? PurchaseRequest { get; set; }
    public Quotation? Quotation { get; set; }
    public ICollection<PurchaseOrderDetail> Details { get; set; } = [];
}
