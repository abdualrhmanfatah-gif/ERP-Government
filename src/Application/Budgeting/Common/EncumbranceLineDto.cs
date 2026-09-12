namespace ERP_Government.Application.Budgeting.Common;

public record EncumbranceLineDto(
    int Id,
    int BudgetItemId,
    decimal Amount,
    decimal LiquidatedAmount,
    decimal CancelledAmount,
    string? Description);
