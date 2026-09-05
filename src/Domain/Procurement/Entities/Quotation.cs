using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class Quotation : BaseAuditableEntity
{
    public string QuotationNumber { get; set; } = string.Empty;
    public int RFQId { get; set; }
    public int RFQSupplierId { get; set; }
    public int SupplierId { get; set; }
    public int? PartyId { get; set; }
    public DateTime QuotationDate { get; set; }
    public DateTime? ValidUntil { get; set; }
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
    public int? LeadTimeDays { get; set; }
    public int? WarrantyPeriodMonths { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public string? SelectionReason { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public RequestForQuotation? RequestForQuotation { get; set; }
}
