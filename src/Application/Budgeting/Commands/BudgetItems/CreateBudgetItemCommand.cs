using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsCreate)]
public record CreateBudgetItemCommand(
    int BudgetId,
    string ItemCode,
    string ItemName,
    int? ParentId,
    int? AccountId,
    int? FundId,
    int? CostCenterId,
    int? BudgetClassificationId,
    string? Remarks,
    bool? AllowOverrun) : IRequest<Result<int>>;

public class CreateBudgetItemCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBudgetItemCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateBudgetItemCommand request,
        CancellationToken cancellationToken)
    {
        var budgetExists = await context.Budgets
            .AnyAsync(x => x.Id == request.BudgetId, cancellationToken);

        if (!budgetExists)
            return Result<int>.Failure(["Budget not found."]);

        var budget = await context.Budgets
            .FirstOrDefaultAsync(x => x.Id == request.BudgetId, cancellationToken);

        if (budget is not null && budget.Status != BudgetStatus.Draft)
            return Result<int>.Failure(["Only Draft budgets can have items added."]);

        var exists = await context.BudgetItems
            .AnyAsync(x => x.BudgetId == request.BudgetId && x.ItemCode == request.ItemCode, cancellationToken);

        if (exists)
            return Result<int>.Failure(["Budget item code already exists within this budget."]);

        if (request.ParentId.HasValue)
        {
            var parentExists = await context.BudgetItems
                .AnyAsync(x => x.Id == request.ParentId.Value && x.BudgetId == request.BudgetId, cancellationToken);

            if (!parentExists)
                return Result<int>.Failure(["Parent budget item not found in the same budget."]);
        }

        var entity = new Domain.Budgeting.Entities.BudgetItem
        {
            BudgetId = request.BudgetId,
            ItemCode = request.ItemCode,
            ItemName = request.ItemName,
            ParentId = request.ParentId,
            AccountId = request.AccountId,
            CostCenterId = request.CostCenterId,
            BudgetClassificationId = request.BudgetClassificationId,
            Remarks = request.Remarks,
            AllowOverrun = request.AllowOverrun,
        };

        context.BudgetItems.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateBudgetItemCommandValidator : AbstractValidator<CreateBudgetItemCommand>
{
    public CreateBudgetItemCommandValidator()
    {
        RuleFor(x => x.BudgetId)
            .GreaterThan(0).WithMessage("Budget ID must be greater than 0.");

        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage("Item code is required.");

        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item name is required.");
    }
}
