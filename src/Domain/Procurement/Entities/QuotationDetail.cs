using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class QuotationDetail : BaseAuditableEntity
{
    public int QuotationId { get; set; }
    public int ItemId { get; set; }
    public int UnitId { get; set; }
    public decimal? ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? NetUnitPrice { get; set; }
    public decimal? LineTotal { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineTotalWithTax { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Quotation? Quotation { get; set; }
}
