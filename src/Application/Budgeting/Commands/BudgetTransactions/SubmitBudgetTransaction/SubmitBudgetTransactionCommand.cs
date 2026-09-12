using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.SubmitBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsSubmit)]
public record SubmitBudgetTransactionCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class SubmitBudgetTransactionCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<SubmitBudgetTransactionCommand, Result>
{
    public async Task<Result> Handle(SubmitBudgetTransactionCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.BudgetTransactions.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Budget transaction not found."]);

        if (entity.Status != BudgetTransactionStatus.Draft)
            return Result.Failure(["Only Draft transactions can be submitted."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        entity.Status = BudgetTransactionStatus.Submitted;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "BudgetTransaction",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Draft -> Submitted",
            DecisionAt = DateTimeOffset.UtcNow
        });

        await statusLogger.LogAsync(
            "budgettransactions",
            entity.Id,
            BudgetTransactionStatus.Draft.ToString(),
            BudgetTransactionStatus.Submitted.ToString(),
            userId,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
