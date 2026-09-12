using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItemAllocations;

[Authorize(Policy = PermissionCodes.BudgetItemAllocationsDelete)]
public record DeleteBudgetItemAllocationCommand(int Id) : IRequest<Result>;

public class DeleteBudgetItemAllocationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteBudgetItemAllocationCommand, Result>
{
    public async Task<Result> Handle(
        DeleteBudgetItemAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetItemAllocations
            .Include(x => x.Budget)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["المخصص غير موجود"]);

        if (entity.Budget.Status != BudgetStatus.Draft)
            return Result.Failure(["لا يمكن حذف المخصص إلا في الموازنة المسودة"]);

        var hasLinkedTransactions = await context.BudgetTransactions
            .AnyAsync(x => x.BudgetItemAllocationId == request.Id, cancellationToken);

        if (hasLinkedTransactions)
            return Result.Failure(["لا يمكن حذف المخصص لوجود معاملات مرتبطة به"]);

        context.BudgetItemAllocations.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
