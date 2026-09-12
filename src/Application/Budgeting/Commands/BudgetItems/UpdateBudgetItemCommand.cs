using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsUpdate)]
public record UpdateBudgetItemCommand(
    int Id,
    string ItemName,
    int? AccountId,
    int? CostCenterId,
    int? BudgetClassificationId,
    bool? AllowOverrun,
    string? Remarks,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateBudgetItemCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateBudgetItemCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBudgetItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetItems
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget item not found."]);

        var budget = await context.Budgets
            .FindAsync(entity.BudgetId, cancellationToken);

        if (budget is null || budget.Status != BudgetStatus.Draft)
            return Result.Failure(["Only items in Draft budgets can be updated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        entity.ItemName = request.ItemName;
        entity.AccountId = request.AccountId;
        entity.CostCenterId = request.CostCenterId;
        entity.BudgetClassificationId = request.BudgetClassificationId;
        entity.AllowOverrun = request.AllowOverrun;
        entity.Remarks = request.Remarks;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateBudgetItemCommandValidator : AbstractValidator<UpdateBudgetItemCommand>
{
    public UpdateBudgetItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid budget item ID.");

        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item name is required.");
    }
}
