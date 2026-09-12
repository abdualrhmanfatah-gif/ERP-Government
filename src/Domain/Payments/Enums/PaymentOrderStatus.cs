namespace ERP_Government.Domain.Payments.Enums;

public enum PaymentOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    SentToTreasury = 3,
    Paid = 4,
    Cancelled = 5,
    Rejected = 6,
    Voided = 7
}
