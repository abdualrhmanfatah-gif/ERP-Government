using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItemAllocations;

[Authorize(Policy = PermissionCodes.BudgetItemAllocationsCreate)]
public record CreateBudgetItemAllocationCommand(
    int BudgetId,
    int BudgetItemId,
    decimal ProposedAmount,
    string? Remarks) : IRequest<Result<int>>;

public class CreateBudgetItemAllocationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBudgetItemAllocationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateBudgetItemAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var budget = await context.Budgets
            .FirstOrDefaultAsync(x => x.Id == request.BudgetId, cancellationToken);

        if (budget is null)
            return Result<int>.Failure(["الموازنة غير موجودة"]);

        if (budget.Status != BudgetStatus.Draft)
            return Result<int>.Failure(["لا يمكن إضافة مخصصات إلا في الموازنة المسودة"]);

        var budgetItem = await context.BudgetItems
            .FirstOrDefaultAsync(x => x.Id == request.BudgetItemId && x.BudgetId == request.BudgetId, cancellationToken);

        if (budgetItem is null)
            return Result<int>.Failure(["البند غير موجود في هذه الموازنة"]);

        if (!budgetItem.IsActive)
            return Result<int>.Failure(["البند غير نشط"]);

        var duplicateExists = await context.BudgetItemAllocations
            .AnyAsync(x => x.BudgetId == request.BudgetId && x.BudgetItemId == request.BudgetItemId, cancellationToken);

        if (duplicateExists)
            return Result<int>.Failure(["هذا البند مسجل بالفعل في الموازنة"]);

        if (budgetItem.AccountId.HasValue)
        {
            var fiscalYearId = budget.FiscalYearId;
            var sharedAccountExists = await context.BudgetItemAllocations
                .AnyAsync(x => x.BudgetId == request.BudgetId
                    && x.BudgetItemId != request.BudgetItemId
                    && x.BudgetItem.AccountId == budgetItem.AccountId
                    && x.Budget.FiscalYearId == fiscalYearId, cancellationToken);

            if (sharedAccountExists)
                return Result<int>.Failure(["الحساب مرتبط ببند آخر في هذه الموازنة"]);
        }

        var entity = new BudgetItemAllocation
        {
            BudgetId = request.BudgetId,
            BudgetItemId = request.BudgetItemId,
            ProposedAmount = request.ProposedAmount,
            Remarks = request.Remarks,
        };

        context.BudgetItemAllocations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateBudgetItemAllocationCommandValidator : AbstractValidator<CreateBudgetItemAllocationCommand>
{
    public CreateBudgetItemAllocationCommandValidator()
    {
        RuleFor(x => x.BudgetId)
            .GreaterThan(0).WithMessage("Budget ID must be greater than 0.");

        RuleFor(x => x.BudgetItemId)
            .GreaterThan(0).WithMessage("Budget Item ID must be greater than 0.");

        RuleFor(x => x.ProposedAmount)
            .GreaterThanOrEqualTo(0).WithMessage("المبلغ المقترح لا يمكن أن يكون سالبًا");
    }
}
