using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public record DisbursementRequestPrintDto
{
    public string RequestNumber { get; init; } = string.Empty;
    public DateOnly RequestDate { get; init; }
    public string RequestedByName { get; init; } = string.Empty;
    public string BeneficiaryName { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencyName { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string FiscalYearName { get; init; } = string.Empty;
    public int FiscalYearNumber { get; init; }
    public DisbursementRequestStatus Status { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public string? Notes { get; init; }

    public int? AccrualJournalEntryId { get; init; }
    public string? AccrualEntryNumber { get; init; }
    public decimal? AccrualAmount { get; init; }
    public string? AccrualExpenseAccountCode { get; init; }
    public string? AccrualExpenseAccountName { get; init; }
    public string? AccrualLiabilityAccountCode { get; init; }
    public string? AccrualLiabilityAccountName { get; init; }

    public int? PaymentOrderId { get; init; }
    public string? PaymentOrderNumber { get; init; }

    public DateTimeOffset Created { get; init; }
    public string? CreatedByName { get; init; }
    public List<ApprovalPrintDto> Approvals { get; init; } = [];
}

public record ApprovalPrintDto
{
    public int Step { get; init; }
    public string ApproverName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public DateTimeOffset DecisionAt { get; init; }
    public decimal? ApprovedAmount { get; init; }
    public string? IssuingAuthorityName { get; init; }
    public string? IssuingAuthorityCapacity { get; init; }
}
