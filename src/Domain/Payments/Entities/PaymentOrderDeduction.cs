using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Payments.Entities;

public class PaymentOrderDeduction : BaseAuditableEntity
{
    public int PaymentOrderId { get; set; }
    public int LineNumber { get; set; }
    public Enums.DeductionType DeductionType { get; set; }
    public string? DeductionCode { get; set; }
    public string? Description { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal? DeductionPercent { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsTaxDeduction { get; set; }
    public int? TaxAuthorityId { get; set; }
    public string? ReferenceNumber { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PaymentOrder PaymentOrder { get; set; } = null!;
}
