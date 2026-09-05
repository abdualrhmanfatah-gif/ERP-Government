using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class PurchaseOrderDetail : BaseAuditableEntity
{
    public int PurchaseOrderId { get; set; }
    public int? PurchaseRequestDetailId { get; set; }
    public int? QuotationDetailId { get; set; }
    public int ItemId { get; set; }
    public int UnitId { get; set; }
    public decimal? ConversionFactor { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? RemainingQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? NetUnitPrice { get; set; }
    public decimal? LineTotal { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineTotalWithTax { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PurchaseOrder? PurchaseOrder { get; set; }
}
