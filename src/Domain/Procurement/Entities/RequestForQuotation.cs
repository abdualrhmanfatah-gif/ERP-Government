using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class RequestForQuotation : BaseAuditableEntity
{
    public string RFQNumber { get; set; } = string.Empty;
    public DateTime RFQDate { get; set; }
    public int? PurchaseRequestId { get; set; }
    public DateTime? DeadlineDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? TermsAndConditions { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
