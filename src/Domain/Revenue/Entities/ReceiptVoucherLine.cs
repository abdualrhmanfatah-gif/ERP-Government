using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Revenue.Entities;

public class ReceiptVoucherLine : BaseAuditableEntity
{
    public int ReceiptVoucherId { get; set; }
    public int RevenueAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ReceiptVoucher ReceiptVoucher { get; set; } = null!;
    public Account RevenueAccount { get; set; } = null!;
}
