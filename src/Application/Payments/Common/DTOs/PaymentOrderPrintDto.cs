using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public class PaymentOrderPrintDto
{
    public string OrderNumber { get; init; } = string.Empty;
    public DateOnly OrderDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string OrderType { get; init; } = string.Empty;
    public PaymentOrderStatus Status { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public string? DisbursementRequestNumber { get; init; }

    public int? AccrualJournalEntryId { get; init; }
    public string? AccrualEntryNumber { get; init; }
    public decimal? AccrualAmount { get; init; }
    public string? AccrualExpenseAccountCode { get; init; }
    public string? AccrualExpenseAccountName { get; init; }
    public string? AccrualLiabilityAccountCode { get; init; }
    public string? AccrualLiabilityAccountName { get; init; }

    public string FiscalYearName { get; init; } = string.Empty;
    public int FiscalYearNumber { get; init; }

    public decimal AmountGross { get; init; }
    public decimal TaxDeductions { get; init; }
    public decimal OtherDeductions { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal NetAmount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencyName { get; init; } = string.Empty;

    public string BeneficiaryName { get; init; } = string.Empty;
    public string? BeneficiaryAccountNumber { get; init; }
    public string? BeneficiaryBankName { get; init; }

    public string Purpose { get; init; } = string.Empty;
    public int AttachmentsCount { get; init; }

    public string FundCode { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;
    public string? ClassificationCode { get; init; }
    public string? ClassificationName { get; init; }
    public string? AccountCode { get; init; }
    public string? AccountName { get; init; }
    public string? CostCenterCode { get; init; }
    public string? CostCenterName { get; init; }

    public string? BudgetItemCode { get; init; }
    public string? BudgetItemName { get; init; }
    public decimal? AllocationProposedAmount { get; init; }
    public decimal? AllocationApprovedAmount { get; init; }

    public string PaymentMethodName { get; init; } = string.Empty;
    public string? PaymentReferenceNumber { get; init; }
    public string? PaymentNumber { get; init; }
    public DateTimeOffset? PaidAt { get; init; }

    public string? CreatedByName { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? ApproverName { get; init; }
    public string? RequiredRole { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string? PaidByName { get; init; }

    public bool IsUnapproved { get; init; }
}
