using ERP_Government.Domain.Common;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Domain.Procurement.Entities;

public class Quotation : BaseAuditableEntity
{
    public string QuotationNumber { get; set; } = string.Empty;
    public int SupplierPartyId { get; set; }
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
    public QuotationStatus Status { get; set; }
    public decimal? TechnicalScore { get; set; }
    public decimal? FinancialScore { get; set; }
    public string? SelectionReason { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<QuotationDetail> Details { get; set; } = [];
}
