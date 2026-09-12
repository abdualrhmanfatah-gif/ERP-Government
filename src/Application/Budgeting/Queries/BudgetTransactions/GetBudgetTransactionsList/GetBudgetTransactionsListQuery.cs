using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.BudgetTransactions.GetBudgetTransactionsList;

[Authorize(Policy = PermissionCodes.BudgetTransactionsView)]
public record GetBudgetTransactionsListQuery(
    int? BudgetItemAllocationId = null,
    BudgetTransactionType? TransactionType = null,
    BudgetTransactionStatus? Status = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null) : IRequest<List<BudgetTransactionListItemDto>>;

public class GetBudgetTransactionsListQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetBudgetTransactionsListQuery, List<BudgetTransactionListItemDto>>
{
    public async Task<List<BudgetTransactionListItemDto>> Handle(
        GetBudgetTransactionsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.BudgetTransactions
            .Include(t => t.BudgetItemAllocation)
            .AsQueryable();

        if (request.BudgetItemAllocationId.HasValue)
            query = query.Where(t => t.BudgetItemAllocationId == request.BudgetItemAllocationId.Value);

        if (request.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == request.TransactionType.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(t => t.TransactionDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(t => t.TransactionDate <= request.DateTo.Value);

        var items = await query
            .OrderByDescending(t => t.Created)
            .ToListAsync(cancellationToken);

        var reversedIds = await context.BudgetTransactions
            .Where(t => t.ReversalOfId != null)
            .Select(t => t.ReversalOfId!.Value)
            .ToListAsync(cancellationToken);

        return items.Select(t => new BudgetTransactionListItemDto
        {
            Id = t.Id,
            TransactionNumber = t.TransactionNumber,
            BudgetItemAllocationId = t.BudgetItemAllocationId,
            TransactionType = t.TransactionType,
            TransactionDate = t.TransactionDate,
            Amount = t.Amount,
            Direction = t.Direction,
            Description = t.Description,
            Status = t.Status,
            IsReversed = reversedIds.Contains(t.Id),
            ReversalOfId = t.ReversalOfId,
            RowVersion = t.RowVersion,
            Created = t.Created
        }).ToList();
    }
}
