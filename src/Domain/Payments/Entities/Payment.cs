using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Payments.Entities;

public class Payment : BaseAuditableEntity
{
    public string PaymentNumber { get; set; } = string.Empty;
    public int PaymentOrderId { get; set; }
    public int DisbursementRequestId { get; set; }
    public Enums.PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public int PaidById { get; set; }
    public string PaidByName { get; set; } = string.Empty;
    public DateTimeOffset PaidAt { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public Enums.PaymentStatus Status { get; set; }
    public string? PayeeName { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PaymentOrder PaymentOrder { get; set; } = null!;
}
