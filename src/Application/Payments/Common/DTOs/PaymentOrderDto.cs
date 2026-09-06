using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public class PaymentOrderDto
{
    public int Id { get; init; }
    public string PaymentOrderNumber { get; init; } = string.Empty;
    public DateTime PaymentOrderDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string PaymentOrderType { get; init; } = string.Empty;
    public int VendorId { get; init; }
    public int FundId { get; init; }
    public int FiscalYearId { get; init; }
    public int AppropriationId { get; init; }
    public int? BudgetClassificationId { get; init; }
    public int? CostCenterId { get; init; }
    public int? ProjectId { get; init; }
    public int? PurchaseOrderId { get; init; }
    public int? EncumbranceId { get; init; }
    public int CurrencyId { get; init; }
    public decimal? ExchangeRate { get; init; }
    public decimal AmountGross { get; init; }
    public decimal DeductionAmount { get; init; }
    public ERP_Government.Domain.Payments.Enums.PaymentMethod? PaymentMethod { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public int? BankAccountId { get; init; }
    public string BeneficiaryName { get; init; } = string.Empty;
    public string? BeneficiaryIban { get; init; }
    public string? BeneficiaryAccountNumber { get; init; }
    public string? BeneficiaryBankName { get; init; }
    public PaymentOrderStatus Status { get; init; }
    public BudgetCheckStatus BudgetCheckStatus { get; init; }
    public string? TreasuryStatus { get; init; }
    public string? TreasuryReference { get; init; }
    public DateTimeOffset? TreasurySentAt { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public int? JournalEntryId { get; init; }
    public string? Notes { get; init; }
    public List<PaymentOrderLineDto> Lines { get; set; } = [];
    public List<PaymentOrderDeductionDto> Deductions { get; set; } = [];
}

public class PaymentOrderLineDto
{
    public int Id { get; init; }
    public int LineNumber { get; init; }
    public PaymentOrderLineType LineType { get; init; }
    public string? Description { get; init; }
    public int AccountId { get; init; }
    public decimal Amount { get; init; }
    public decimal? TaxAmount { get; init; }
    public int? FundId { get; init; }
    public int? AppropriationId { get; init; }
    public int? OrganizationUnitId { get; init; }
    public int? CostCenterId { get; init; }
    public int? ProjectId { get; init; }
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
