using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class EncumbranceLine : BaseEntity
{
    public int EncumbranceId { get; set; }
    public int BudgetItemId { get; set; }
    public decimal Amount { get; set; }
    public decimal LiquidatedAmount { get; set; } = 0;
    public decimal CancelledAmount { get; set; } = 0;
    public string? Description { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Encumbrance Encumbrance { get; set; } = null!;
    public BudgetItem BudgetItem { get; set; } = null!;
}
