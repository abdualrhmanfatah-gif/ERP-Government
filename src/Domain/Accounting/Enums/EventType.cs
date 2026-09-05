namespace ERP_Government.Domain.Accounting.Enums;

public enum EventType
{
    ReceiptCollection = 0,
    DepositClearing = 1,
    PaymentExecution = 2,
    Reversal = 3,
    Other = 4,
    PurchaseOrderApproved = 5,
    GoodsReceiptNoteApproved = 6,
    BankReconciliationPosted = 7,
    RevenueReceiptPosted = 8,
    JournalEntryPosted = 9,
    DepreciationPosted = 10,
    PaymentOrderExecuted = 11,
    ReceiptVoucherCollected = 12,
    CheckCleared = 13,
    ClosingEntry = 14
}
