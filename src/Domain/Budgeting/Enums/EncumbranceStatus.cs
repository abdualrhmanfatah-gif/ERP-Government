namespace ERP_Government.Domain.Budgeting.Enums;

public enum EncumbranceStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Active = 3,
    PartiallyReleased = 4,
    PartiallyLiquidated = 5,
    FullyLiquidated = 6,
    Closed = 7,
    Cancelled = 8,
    Reversed = 9,
    Suspended = 10
}
