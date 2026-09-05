using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Payments.Entities;

public class DisbursementRequest : BaseAuditableEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public int PaymentOrderId { get; set; }
    public int RequestedById { get; set; }
    public DateOnly RequestDate { get; set; }
    public Enums.DisbursementRequestStatus Status { get; set; }
    public bool HasWarning { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PaymentOrder PaymentOrder { get; set; } = null!;
}
