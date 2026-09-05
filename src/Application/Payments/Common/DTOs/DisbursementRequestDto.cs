using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public record DisbursementRequestDto(
    int Id,
    string RequestNumber,
    int PaymentOrderId,
    string PaymentOrderNumber,
    int RequestedById,
    string RequestedByName,
    DateOnly RequestDate,
    DisbursementRequestStatus Status,
    bool HasWarning,
    string? Notes,
    decimal RequestedAmount,
    string? PayeeName,
    string? FundName,
    DateTimeOffset? ApprovalDate,
    DateTimeOffset? PaymentDate);

public record DisbursementRequestDetailDto(
    int Id,
    string RequestNumber,
    int PaymentOrderId,
    string PaymentOrderNumber,
    int RequestedById,
    string RequestedByName,
    DateOnly RequestDate,
    DisbursementRequestStatus Status,
    bool HasWarning,
    string? Notes,
    decimal RequestedAmount,
    List<ApprovalStepDto> Approvals);

public record ApprovalStepDto(
    int Step,
    int ApproverUserId,
    string ApproverName,
    string Role,
    string Decision,
    DateTimeOffset DecisionAt);
