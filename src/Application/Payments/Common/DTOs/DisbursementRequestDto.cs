using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public record DisbursementRequestDto(
    int Id,
    string RequestNumber,
    int RequestedById,
    string RequestedByName,
    string BeneficiaryName,
    decimal RequestedAmount,
    int CurrencyId,
    string Purpose,
    int FinancialYearId,
    DateOnly RequestDate,
    DisbursementRequestStatus Status,
    string? Notes,
    DateTimeOffset? PaymentDate,
    int? PaymentOrderId,
    string? PaymentOrderNumber,
    int? AccrualJournalEntryId,
    string? AccrualEntryNumber,
    List<ApprovalStepDto> Approvals);

public record DisbursementRequestDetailDto(
    int Id,
    string RequestNumber,
    int RequestedById,
    string RequestedByName,
    string BeneficiaryName,
    decimal RequestedAmount,
    int CurrencyId,
    string Purpose,
    int FinancialYearId,
    DateOnly RequestDate,
    DisbursementRequestStatus Status,
    string? Notes,
    DateTimeOffset? PaymentDate,
    int? PaymentOrderId,
    string? PaymentOrderNumber,
    int? AccrualJournalEntryId,
    string? AccrualEntryNumber,
    List<ApprovalStepDto> Approvals);

public record ApprovalStepDto(
    int Step,
    int ApproverUserId,
    string ApproverName,
    string Role,
    string Decision,
    DateTimeOffset DecisionAt,
    decimal? ApprovedAmount);
