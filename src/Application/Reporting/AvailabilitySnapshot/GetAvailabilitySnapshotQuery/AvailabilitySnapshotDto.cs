namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotQuery;

public record AvailabilitySnapshotDto
{
    public int BudgetItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public string ControlState { get; init; } = string.Empty;
    public List<AvailabilitySnapshotLineDto> Breakdown { get; init; } = [];
    public AvailabilitySnapshotTotalDto Totals { get; init; } = new();
}

public record AvailabilitySnapshotLineDto
{
    public int FundId { get; init; }
    public string FundCode { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;
    public int? ProgramId { get; init; }
    public string? ProgramCode { get; init; }
    public int? ProjectId { get; init; }
    public string? ProjectCode { get; init; }
    public decimal AppropriationAmount { get; init; }
    public decimal EncumberedAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal AvailableAmount { get; init; }
}

public record AvailabilitySnapshotTotalDto
{
    public decimal AppropriationAmount { get; init; }
    public decimal EncumberedAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal AvailableAmount { get; init; }
}
