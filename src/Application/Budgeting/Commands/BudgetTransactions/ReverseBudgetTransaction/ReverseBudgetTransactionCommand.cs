using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ReverseBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsReverse)]
public record ReverseBudgetTransactionCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result<int>>;

public class ReverseBudgetTransactionCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ReverseBudgetTransactionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ReverseBudgetTransactionCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(["User identity is required for this operation."]);

        var entity = await context.BudgetTransactions.FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<int>.Failure(["Budget transaction not found."]);

        if (entity.Status != BudgetTransactionStatus.Posted)
            return Result<int>.Failure(["Only Posted transactions can be reversed."]);

        if (entity.TransactionType == BudgetTransactionType.Reversal)
            return Result<int>.Failure(["Reversal transactions cannot be reversed (no double-reversals)."]);

        if (entity.ReversalOfId.HasValue)
            return Result<int>.Failure(["Reversal transactions cannot be reversed (no double-reversals)."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result<int>.Failure(["Concurrency conflict."]);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result<int>.Failure(["Reversal reason is required."]);

        entity.Status = BudgetTransactionStatus.Reversed;

        context.ApprovalHistory.Add(new ERP_Government.Domain.Security.Entities.ApprovalHistory
        {
            DocumentType = "BudgetTransaction",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Posted -> Reversed",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "budgettransactions",
            entity.Id,
            BudgetTransactionStatus.Posted.ToString(),
            BudgetTransactionStatus.Reversed.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        var reversalNumber = await sequenceService.GenerateNextNumberAsync("BudgetTransaction", cancellationToken);

        var reversalDirection = entity.Direction == TransactionDirection.Increase
            ? TransactionDirection.Decrease
            : TransactionDirection.Increase;

        var reversal = new Domain.Budgeting.Entities.BudgetTransaction
        {
            TransactionNumber = reversalNumber,
            BudgetId = entity.BudgetId,
            BudgetItemAllocationId = entity.BudgetItemAllocationId,
            TransactionType = BudgetTransactionType.Reversal,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = entity.Amount,
            Direction = reversalDirection,
            DocumentType = entity.DocumentType,
            DocumentId = entity.DocumentId,
            Description = $"Reversal of {entity.TransactionNumber}: {request.Reason}",
            Status = BudgetTransactionStatus.Posted,
            PostedAt = DateTimeOffset.UtcNow,
            PostedBy = userId.ToString(),
            ReversalOfId = entity.Id,
            ReversalReason = request.Reason
        };

        context.BudgetTransactions.Add(reversal);

        var allocation = await context.BudgetItemAllocations
            .FirstOrDefaultAsync(a => a.Id == entity.BudgetItemAllocationId, cancellationToken);

        if (allocation is not null && allocation.ApprovedAmount.HasValue)
        {
            if (entity.Direction == TransactionDirection.Increase)
                allocation.ApprovedAmount -= entity.Amount;
            else
                allocation.ApprovedAmount += entity.Amount;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(reversal.Id);
    }
}
