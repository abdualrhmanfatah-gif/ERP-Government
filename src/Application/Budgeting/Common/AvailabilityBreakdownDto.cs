namespace ERP_Government.Application.Budgeting.Common;

public record AvailabilityBreakdownDto(
    int FundId,
    string FundCode,
    string FundName,
    int? ProgramId,
    string? ProgramCode,
    string? ProgramName,
    int? ProjectId,
    string? ProjectCode,
    string? ProjectName,
    int BudgetItemId,
    string ItemCode,
    decimal AppropriationAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);
