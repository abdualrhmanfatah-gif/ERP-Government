using ERP_Government.Domain.Common;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Domain.Revenue.Entities;

public class ReceiptVoucher : BaseAuditableEntity
{
    public int CollectionOrderId { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateOnly VoucherDate { get; set; }
    public int PartyId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string ReceivedFrom { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? DepositSlip47Id { get; set; }
    public ReceiptVoucherStatus Status { get; set; } = ReceiptVoucherStatus.Draft;
    public int? ApprovedById { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? CancellationReason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public CollectionOrder CollectionOrder { get; set; } = null!;
    public Party? Party { get; set; }
    public DepositSlip47? DepositSlip47 { get; set; }
    public ICollection<ReceiptVoucherLine> Lines { get; set; } = new List<ReceiptVoucherLine>();
    public ICollection<Check> Checks { get; set; } = new List<Check>();
}
