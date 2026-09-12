using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Payments.Entities;

public class PaymentOrder : BaseAuditableEntity
{
    public string PaymentOrderNumber { get; set; } = string.Empty;
    public DateOnly PaymentOrderDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string PaymentOrderType { get; set; } = string.Empty;
    public int FundId { get; set; }
    public int FiscalYearId { get; set; }
    public int? BudgetItemAllocationId { get; set; }
    public int? EncumbranceLineId { get; set; }
    public int? BudgetClassificationId { get; set; }
    public int? CostCenterId { get; set; }
    public int? AccountId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public int? EncumbranceId { get; set; }
    public int CurrencyId { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal AmountGross { get; set; }
    public decimal DeductionAmount { get; set; }
    public Enums.PaymentMethod? PaymentMethod { get; set; }
    public int? BankAccountId { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public string? BeneficiaryAccountNumber { get; set; }
    public string? BeneficiaryBankName { get; set; }
    public Enums.PaymentOrderStatus Status { get; set; }
    public DateTimeOffset? TreasurySentAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public int? JournalEntryId { get; set; }
    public string? Notes { get; set; }
    public int? DisbursementRequestId { get; set; }
    public int? AccrualJournalEntryId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public DisbursementRequest? DisbursementRequest { get; set; }
    public ERP_Government.Domain.Accounting.Entities.JournalEntry? AccrualJournalEntry { get; set; }
}
