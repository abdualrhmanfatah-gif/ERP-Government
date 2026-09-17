namespace ERP_Government.Domain.Revenue.Enums;

public enum CollectionOrderStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    PartiallyCollected = 3,
    Collected = 4,
    Cancelled = 5
}
