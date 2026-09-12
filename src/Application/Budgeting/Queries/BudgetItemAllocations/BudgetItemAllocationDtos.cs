namespace ERP_Government.Application.Budgeting.Queries.BudgetItemAllocations;

public class BudgetItemAllocationDto
{
    public int Id { get; init; }
    public int BudgetId { get; init; }
    public int BudgetItemId { get; init; }
    public string BudgetItemCode { get; init; } = string.Empty;
    public string BudgetItemName { get; init; } = string.Empty;
    public int? AccountId { get; init; }
    public int? CostCenterId { get; init; }
    public int? BudgetClassificationId { get; init; }
    public decimal ProposedAmount { get; init; }
    public decimal? ApprovedAmount { get; init; }
    public string? Remarks { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class BudgetItemAllocationDetailDto : BudgetItemAllocationDto
{
    public decimal ActualExpenditure { get; init; }
    public decimal OutstandingEncumbrance { get; init; }
    public decimal? RemainingAmount { get; init; }
    public decimal? AvailableAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}
