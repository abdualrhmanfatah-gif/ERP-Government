using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class Appropriation : BaseAuditableEntity
{
    public string AppropriationNumber { get; set; } = string.Empty;
    public int BudgetId { get; set; }
    public int BudgetItemId { get; set; }
    public int? TargetBudgetItemId { get; set; }
    public Enums.AppropriationType AppropriationType { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public decimal Amount { get; set; }
    public Enums.AppropriationStatus Status { get; set; } = Enums.AppropriationStatus.Draft;
    public byte[] RowVersion { get; set; } = [];

    public Budget Budget { get; set; } = null!;
    public BudgetItem BudgetItem { get; set; } = null!;
}
