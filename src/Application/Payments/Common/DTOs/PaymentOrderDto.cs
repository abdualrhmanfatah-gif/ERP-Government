using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public class PaymentOrderDto
{
    public int Id { get; init; }
    public string PaymentOrderNumber { get; init; } = string.Empty;
    public DateTime PaymentOrderDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string PaymentOrderType { get; init; } = string.Empty;
    public int FundId { get; init; }
    public int FiscalYearId { get; init; }
    public int? BudgetItemAllocationId { get; init; }
    public int? BudgetClassificationId { get; init; }
    public int? CostCenterId { get; init; }
    public int? AccountId { get; init; }
    public int? PurchaseOrderId { get; init; }
    public int? EncumbranceId { get; init; }
    public int CurrencyId { get; init; }
    public decimal? ExchangeRate { get; init; }
    public decimal AmountGross { get; init; }
    public decimal DeductionAmount { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public int? BankAccountId { get; init; }
    public string BeneficiaryName { get; init; } = string.Empty;
    public string? BeneficiaryAccountNumber { get; init; }
    public string? BeneficiaryBankName { get; init; }
    public PaymentOrderStatus Status { get; init; }
    public DateTimeOffset? TreasurySentAt { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public int? JournalEntryId { get; init; }
    public string? Notes { get; init; }
    public int? DisbursementRequestId { get; init; }
    public string? DisbursementRequestNumber { get; init; }
    public byte[]? RowVersion { get; init; }
    public List<PaymentOrderDeductionDto> Deductions { get; set; } = [];
}

public class PaymentOrderDeductionDto
{
    public int Id { get; init; }
    public int LineNumber { get; init; }
    public DeductionType DeductionType { get; init; }
    public string? DeductionCode { get; init; }
    public string? Description { get; init; }
    public int AccountId { get; init; }
    public decimal Amount { get; init; }
    public decimal? DeductionPercent { get; init; }
    public bool IsMandatory { get; init; }
    public bool IsTaxDeduction { get; init; }
    public int? TaxAuthorityId { get; init; }
    public string? ReferenceNumber { get; init; }
}
