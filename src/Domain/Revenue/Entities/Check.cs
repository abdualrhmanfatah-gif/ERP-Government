using ERP_Government.Domain.Common;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Domain.Revenue.Entities;

public class Check : BaseAuditableEntity
{
    public int ReceiptVoucherId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string CheckNumber { get; set; } = string.Empty;
    public DateOnly CheckDate { get; set; }
    public decimal Amount { get; set; }
    public CheckStatus Status { get; set; } = CheckStatus.Received;
    public int? DepositSlip48Id { get; set; }
    public DateTimeOffset? ClearedAt { get; set; }
    public DateTimeOffset? BouncedAt { get; set; }
    public int? ReplacementVoucherId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ReceiptVoucher ReceiptVoucher { get; set; } = null!;
    public DepositSlip48? DepositSlip48 { get; set; }
    public ReceiptVoucher? ReplacementVoucher { get; set; }
}
