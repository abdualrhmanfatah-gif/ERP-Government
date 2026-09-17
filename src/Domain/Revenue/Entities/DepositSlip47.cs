using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Revenue.Entities;

public class DepositSlip47 : BaseAuditableEntity
{
    public string SlipNumber { get; set; } = string.Empty;
    public DateOnly SlipDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int? ApprovedById { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<ReceiptVoucher> ReceiptVouchers { get; set; } = new List<ReceiptVoucher>();
}
