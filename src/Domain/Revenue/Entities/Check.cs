using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Revenue.Entities;

public class Check : BaseAuditableEntity
{
    public int ReceiptVoucherId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string CheckNumber { get; set; } = string.Empty;
    public DateOnly CheckDate { get; set; }
    public decimal Amount { get; set; }
    public Enums.CheckStatus Status { get; set; }
    public DateTimeOffset? ClearedAt { get; set; }
    public DateTimeOffset? BouncedAt { get; set; }
    public int? ReplacementVoucherId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ReceiptVoucher ReceiptVoucher { get; set; } = null!;
    public ReceiptVoucher? ReplacementVoucher { get; set; }
}
