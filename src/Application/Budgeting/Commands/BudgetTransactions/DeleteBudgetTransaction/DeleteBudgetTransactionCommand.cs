using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.DeleteBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsDelete)]
public record DeleteBudgetTransactionCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class DeleteBudgetTransactionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteBudgetTransactionCommand, Result>
{
    public async Task<Result> Handle(DeleteBudgetTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.BudgetTransactions.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Budget transaction not found."]);

        if (entity.Status != BudgetTransactionStatus.Draft)
            return Result.Failure(["Only Draft transactions can be deleted."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        context.BudgetTransactions.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
