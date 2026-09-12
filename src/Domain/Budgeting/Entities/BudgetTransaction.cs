using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class BudgetTransaction : BaseAuditableEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public int BudgetId { get; set; }
    public int BudgetItemAllocationId { get; set; }
    public Enums.BudgetTransactionType TransactionType { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public Enums.TransactionDirection Direction { get; set; }
    public string? DocumentType { get; set; }
    public int? DocumentId { get; set; }
    public string? Description { get; set; }
    public Enums.BudgetTransactionStatus Status { get; set; } = Enums.BudgetTransactionStatus.Draft;
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedBy { get; set; }
    public int? ReversalOfId { get; set; }
    public string? ReversalReason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Budget Budget { get; set; } = null!;
    public BudgetItemAllocation BudgetItemAllocation { get; set; } = null!;
    public BudgetTransaction? ReversalOf { get; set; }
}
