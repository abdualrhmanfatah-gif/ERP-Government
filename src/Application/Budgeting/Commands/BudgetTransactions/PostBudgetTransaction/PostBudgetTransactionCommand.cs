using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.PostBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsPost)]
public record PostBudgetTransactionCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class PostBudgetTransactionCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<PostBudgetTransactionCommand, Result>
{
    public async Task<Result> Handle(PostBudgetTransactionCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.BudgetTransactions
            .Include(t => t.BudgetItemAllocation)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget transaction not found."]);

        if (entity.Status != BudgetTransactionStatus.Approved)
            return Result.Failure(["Only Approved transactions can be posted."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        if (entity.Direction == TransactionDirection.Decrease && entity.BudgetItemAllocation.ApprovedAmount.HasValue)
        {
            var projectedAmount = entity.BudgetItemAllocation.ApprovedAmount.Value - entity.Amount;
            if (projectedAmount < 0)
                return Result.Failure(["المبلغ المعتمد لا يمكن أن يصبح سالبًا"]);
        }

        if (string.IsNullOrEmpty(entity.TransactionNumber) || entity.TransactionNumber == string.Empty)
        {
            var number = await sequenceService.GenerateNextNumberAsync("BudgetTransaction", cancellationToken);
            entity.TransactionNumber = number;
        }

        if (entity.Direction == TransactionDirection.Increase)
        {
            if (entity.BudgetItemAllocation.ApprovedAmount.HasValue)
                entity.BudgetItemAllocation.ApprovedAmount += entity.Amount;
            else
                entity.BudgetItemAllocation.ApprovedAmount = entity.Amount;
        }
        else
        {
            entity.BudgetItemAllocation.ApprovedAmount -= entity.Amount;
        }

        entity.Status = BudgetTransactionStatus.Posted;
        entity.PostedAt = DateTimeOffset.UtcNow;
        entity.PostedBy = userId.ToString();

        context.ApprovalHistory.Add(new ERP_Government.Domain.Security.Entities.ApprovalHistory
        {
            DocumentType = "BudgetTransaction",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Approved -> Posted",
            DecisionAt = DateTimeOffset.UtcNow
        });

        await statusLogger.LogAsync(
            "budgettransactions",
            entity.Id,
            BudgetTransactionStatus.Approved.ToString(),
            BudgetTransactionStatus.Posted.ToString(),
            userId,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
