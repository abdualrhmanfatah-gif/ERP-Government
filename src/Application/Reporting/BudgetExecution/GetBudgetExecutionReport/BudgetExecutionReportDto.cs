namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;

public record BudgetExecutionReportDto
{
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public List<BudgetExecutionLineDto> Lines { get; init; } = [];
    public BudgetExecutionTotalDto Totals { get; init; } = new();
}

public record BudgetExecutionLineDto
{
    public int BudgetItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public int FundId { get; init; }
    public string FundNumber { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;
    public int? ProgramId { get; init; }
    public string? ProgramCode { get; init; }
    public int? ProjectId { get; init; }
    public string? ProjectCode { get; init; }
    public decimal AppropriatedAmount { get; init; }
    public decimal EncumberedAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal AvailableAmount { get; init; }
}

public record BudgetExecutionTotalDto
{
    public decimal AppropriatedAmount { get; init; }
    public decimal EncumberedAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal AvailableAmount { get; init; }
}
