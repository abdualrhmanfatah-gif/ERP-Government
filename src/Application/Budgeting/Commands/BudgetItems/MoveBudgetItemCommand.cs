using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsMove)]
public record MoveBudgetItemCommand(
    int Id,
    int? NewParentId,
    byte[] RowVersion) : IRequest<Result>;

public class MoveBudgetItemCommandHandler(
    IApplicationDbContext context) : IRequestHandler<MoveBudgetItemCommand, Result>
{
    public async Task<Result> Handle(
        MoveBudgetItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetItems
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget item not found."]);

        var budget = await context.Budgets
            .FindAsync(entity.BudgetId, cancellationToken);

        if (budget is null || budget.Status != BudgetStatus.Draft)
            return Result.Failure(["Only items in Draft budgets can be moved."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        if (request.NewParentId.HasValue)
        {
            var newParent = await context.BudgetItems
                .FindAsync(request.NewParentId.Value, cancellationToken);

            if (newParent is null)
                return Result.Failure(["New parent budget item not found."]);

            if (newParent.BudgetId != entity.BudgetId)
                return Result.Failure(["New parent must belong to the same budget."]);

            // Prevent circular reference: new parent cannot be self or descendant
            if (request.NewParentId == entity.Id)
                return Result.Failure(["Cannot move an item to be its own parent."]);

            if (await IsDescendant(context, request.NewParentId.Value, entity.Id, cancellationToken))
                return Result.Failure(["Cannot move an item under one of its own descendants."]);
        }

        entity.ParentId = request.NewParentId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static async Task<bool> IsDescendant(
        IApplicationDbContext context,
        int ancestorId,
        int itemId,
        CancellationToken cancellationToken)
    {
        // Walk up from ancestor to check if itemId is an ancestor of ancestorId
        var currentId = ancestorId;
        var visited = new HashSet<int> { currentId };

        while (true)
        {
            var item = await context.BudgetItems
                .FindAsync(currentId, cancellationToken);

            if (item?.ParentId is null)
                return false;

            if (item.ParentId == itemId)
                return true;

            if (!visited.Add(item.ParentId.Value))
                return false; // cycle guard

            currentId = item.ParentId.Value;
        }
    }
}
