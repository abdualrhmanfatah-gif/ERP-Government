using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsDelete)]
public record DeleteBudgetItemCommand(
    int Id) : IRequest<Result>;

public class DeleteBudgetItemCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteBudgetItemCommand, Result>
{
    public async Task<Result> Handle(
        DeleteBudgetItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetItems
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget item not found."]);

        var budget = await context.Budgets
            .FindAsync(entity.BudgetId, cancellationToken);

        if (budget is null || budget.Status != BudgetStatus.Draft)
            return Result.Failure(["Only items in Draft budgets can be deleted."]);

        var hasChildren = await context.BudgetItems
            .AnyAsync(x => x.ParentId == request.Id, cancellationToken);

        if (hasChildren)
            return Result.Failure(["Cannot delete a budget item that has children. Delete children first."]);

        var hasAppropriations = await context.Appropriations
            .AnyAsync(x => x.BudgetItemId == request.Id, cancellationToken);

        if (hasAppropriations)
            return Result.Failure(["Cannot delete a budget item that has appropriations."]);

        context.BudgetItems.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
