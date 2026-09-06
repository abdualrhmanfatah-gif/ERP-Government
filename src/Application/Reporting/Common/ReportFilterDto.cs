namespace ERP_Government.Application.Reporting.Common;

public record ReportFilterDto
{
    public int FiscalYearId { get; init; }
    public int? FiscalPeriodId { get; init; }
    public int? FundId { get; init; }
    public int? ProgramId { get; init; }
    public int? ProjectId { get; init; }
    public int? BudgetItemId { get; init; }
    public int? PartyId { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Status { get; init; }
    public int? ApproverId { get; init; }
}
