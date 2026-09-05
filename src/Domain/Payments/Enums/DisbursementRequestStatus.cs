namespace ERP_Government.Domain.Payments.Enums;

public enum DisbursementRequestStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Disbursed = 5,
    Invalidated = 6
}
