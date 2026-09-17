using ERP_Government.Domain.Common;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Domain.Revenue.Entities;

public class RevenueClaim : BaseAuditableEntity
{
    public string ClaimNumber { get; set; } = string.Empty;
    public DateOnly ClaimDate { get; set; }
    public int PartyId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public byte[] RowVersion { get; set; } = [];

    public Party? Party { get; set; }
    public ICollection<CollectionOrder> CollectionOrders { get; set; } = new List<CollectionOrder>();
}
