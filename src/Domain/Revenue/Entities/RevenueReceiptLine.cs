using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Revenue.Entities;

public class RevenueReceiptLine : BaseAuditableEntity
{
    public int ReceiptId { get; set; }
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public RevenueReceipt Receipt { get; set; } = null!;
}
