using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Payments.Entities;

public class PaymentOrderLine : BaseAuditableEntity
{
    public int PaymentOrderId { get; set; }
    public int LineNumber { get; set; }
    public Enums.PaymentOrderLineType LineType { get; set; }
    public string? Description { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal? TaxAmount { get; set; }
    public int? CurrencyId { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? AllocationStatus { get; set; }
    public int? FundId { get; set; }
    public int? AppropriationId { get; set; }
    public int? OrganizationUnitId { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PaymentOrder PaymentOrder { get; set; } = null!;
}
