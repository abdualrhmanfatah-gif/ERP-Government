using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public record PaymentDto(
    int Id,
    string PaymentNumber,
    int DisbursementRequestId,
    string DisbursementRequestNumber,
    int PaymentOrderId,
    string PaymentOrderNumber,
    PaymentMethod PaymentMethod,
    decimal Amount,
    int PaidById,
    string PaidByName,
    DateTimeOffset PaidAt,
    string? ReferenceNumber,
    string? Notes,
    PaymentStatus Status,
    string? PayeeName);
