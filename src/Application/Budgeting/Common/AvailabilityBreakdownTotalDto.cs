namespace ERP_Government.Application.Budgeting.Common;

public record AvailabilityBreakdownTotalDto(
    decimal AppropriationAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);
