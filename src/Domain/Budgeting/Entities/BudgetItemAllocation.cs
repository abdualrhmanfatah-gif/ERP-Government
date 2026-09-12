using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class BudgetItemAllocation : BaseAuditableEntity
{
    public int BudgetId { get; set; }
    public int BudgetItemId { get; set; }
    public decimal ProposedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? Remarks { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Budget Budget { get; set; } = null!;
    public BudgetItem BudgetItem { get; set; } = null!;
}
