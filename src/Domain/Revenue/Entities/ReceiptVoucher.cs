using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Revenue.Entities;

public class ReceiptVoucher : BaseAuditableEntity
{
    public string VoucherNumber { get; set; } = string.Empty;
    public DateOnly VoucherDate { get; set; }
    public int PartyId { get; set; }
    public Enums.PaymentMethod PaymentMethod { get; set; }
    public string ReceivedFrom { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? DepositSlipId { get; set; }
    public Enums.ReceiptVoucherStatus Status { get; set; }
    public int? SubmittedById { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public int? ReviewedById { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? CancellationReason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<ReceiptVoucherLine> Lines { get; set; } = new List<ReceiptVoucherLine>();
    public ICollection<Check> Checks { get; set; } = new List<Check>();
    public Parties.Entities.Party? Party { get; set; }
    public DepositSlip? DepositSlip { get; set; }
}
