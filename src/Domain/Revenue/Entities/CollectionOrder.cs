using ERP_Government.Domain.Common;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Domain.Revenue.Entities;

public class CollectionOrder : BaseAuditableEntity
{
    public int RevenueClaimId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateOnly OrderDate { get; set; }
    public decimal AuthorizedAmount { get; set; }
    public string? Notes { get; set; }
    public CollectionOrderStatus Status { get; set; } = CollectionOrderStatus.Draft;
    public byte[] RowVersion { get; set; } = [];

    public RevenueClaim RevenueClaim { get; set; } = null!;
    public ICollection<ReceiptVoucher> ReceiptVouchers { get; set; } = new List<ReceiptVoucher>();
}
