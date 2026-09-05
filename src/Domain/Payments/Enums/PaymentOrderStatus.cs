namespace ERP_Government.Domain.Payments.Enums;

public enum PaymentOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    SentToTreasury = 3,
    Paid = 4,
    PartiallyPaid = 5,
    Cancelled = 6,
    Rejected = 7,
    Voided = 8
}
