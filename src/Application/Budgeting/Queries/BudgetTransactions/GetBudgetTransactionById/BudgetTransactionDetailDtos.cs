using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.BudgetTransactions.GetBudgetTransactionById;

public class BudgetTransactionDetailDto
{
    public int Id { get; init; }
    public string TransactionNumber { get; init; } = string.Empty;
    public int BudgetItemAllocationId { get; init; }
    public BudgetTransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public TransactionDirection Direction { get; init; }
    public string? DocumentType { get; init; }
    public int? DocumentId { get; init; }
    public string? Description { get; init; }
    public BudgetTransactionStatus Status { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTimeOffset? PostedAt { get; init; }
    public string? PostedBy { get; init; }
    public int? ReversalOfId { get; init; }
    public string? ReversalReason { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
}
