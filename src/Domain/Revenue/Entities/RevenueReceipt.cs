using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Revenue.Entities;

public class RevenueReceipt : BaseAuditableEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public Enums.RevenueReceiptType ReceiptType { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public string? PayerNationalId { get; set; }
    public int FundId { get; set; }
    public int? BudgetClassificationId { get; set; }
    public int CurrencyId { get; set; }
    public decimal AmountTotal { get; set; }
    public Payments.Enums.PaymentMethod PaymentMethod { get; set; }
    public string? ExternalTransactionRef { get; set; }
    public int? JournalEntryId { get; set; }
    public Enums.RevenueReceiptStatus Status { get; set; }
    public string? CancellationReason { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
