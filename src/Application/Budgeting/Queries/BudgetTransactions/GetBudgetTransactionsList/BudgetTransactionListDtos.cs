using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.BudgetTransactions.GetBudgetTransactionsList;

public class BudgetTransactionListItemDto
{
    public int Id { get; init; }
    public string TransactionNumber { get; init; } = string.Empty;
    public int BudgetItemAllocationId { get; init; }
    public BudgetTransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public TransactionDirection Direction { get; init; }
    public string? Description { get; init; }
    public BudgetTransactionStatus Status { get; init; }
    public bool IsReversed { get; set; }
    public int? ReversalOfId { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
}
