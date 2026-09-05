using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class BudgetItemMonthlyPlan : BaseAuditableEntity
{
    public int BudgetItemId { get; set; }
    public int Month { get; set; }
    public decimal PlannedAmount { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public BudgetItem BudgetItem { get; set; } = null!;
}
