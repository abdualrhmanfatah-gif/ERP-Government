using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.BudgetTransactions.GetBudgetTransactionById;

[Authorize(Policy = PermissionCodes.BudgetTransactionsView)]
public record GetBudgetTransactionByIdQuery(int Id) : IRequest<Result<BudgetTransactionDetailDto>>;

public class GetBudgetTransactionByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetBudgetTransactionByIdQuery, Result<BudgetTransactionDetailDto>>
{
    public async Task<Result<BudgetTransactionDetailDto>> Handle(
        GetBudgetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetTransactions
            .Include(t => t.BudgetItemAllocation)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<BudgetTransactionDetailDto>.Failure(ErrorCodes.Budgets.TransactionNotFound, ErrorCategory.NotFound, $"Budget transaction with ID {request.Id} not found.");

        return Result<BudgetTransactionDetailDto>.Success(new BudgetTransactionDetailDto
        {
            Id = entity.Id,
            TransactionNumber = entity.TransactionNumber,
            BudgetItemAllocationId = entity.BudgetItemAllocationId,
            TransactionType = entity.TransactionType,
            TransactionDate = entity.TransactionDate,
            Amount = entity.Amount,
            Direction = entity.Direction,
            DocumentType = entity.DocumentType,
            DocumentId = entity.DocumentId,
            Description = entity.Description,
            Status = entity.Status,
            ApprovedAt = entity.ApprovedAt,
            ApprovedBy = entity.ApprovedBy,
            PostedAt = entity.PostedAt,
            PostedBy = entity.PostedBy,
            ReversalOfId = entity.ReversalOfId,
            ReversalReason = entity.ReversalReason,
            RowVersion = entity.RowVersion,
            Created = entity.Created
        });
    }
}
