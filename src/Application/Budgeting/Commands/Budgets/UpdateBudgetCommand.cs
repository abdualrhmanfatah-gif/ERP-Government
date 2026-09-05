using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsUpdate)]
public record UpdateBudgetCommand(
    int Id,
    string BudgetNumber,
    string BudgetName,
    int FiscalYearId,
    int FundId,
    int BudgetTypeId,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateBudgetCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateBudgetCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBudgetCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Budgets
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget not found."]);

        if (entity.Status != BudgetStatus.Draft)
            return Result.Failure(["Only Draft budgets can be updated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var numberExists = await context.Budgets
            .AnyAsync(x => x.BudgetNumber == request.BudgetNumber && x.Id != request.Id, cancellationToken);

        if (numberExists)
            return Result.Failure(["Budget number already exists."]);

        var fiscalYearExists = await context.FiscalYears
            .AnyAsync(x => x.Id == request.FiscalYearId, cancellationToken);

        if (!fiscalYearExists)
            return Result.Failure(["Fiscal year not found."]);

        var fundExists = await context.Funds
            .AnyAsync(x => x.Id == request.FundId, cancellationToken);

        if (!fundExists)
            return Result.Failure(["Fund not found."]);

        var budgetTypeExists = await context.BudgetTypes
            .AnyAsync(x => x.Id == request.BudgetTypeId, cancellationToken);

        if (!budgetTypeExists)
            return Result.Failure(["Budget type not found."]);

        entity.BudgetNumber = request.BudgetNumber;
        entity.BudgetName = request.BudgetName;
        entity.FiscalYearId = request.FiscalYearId;
        entity.FundId = request.FundId;
        entity.BudgetTypeId = request.BudgetTypeId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid budget ID.");

        RuleFor(x => x.BudgetNumber)
            .NotEmpty().WithMessage("Budget number is required.");

        RuleFor(x => x.BudgetName)
            .NotEmpty().WithMessage("Budget name is required.");

        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Fiscal year ID must be greater than 0.");

        RuleFor(x => x.FundId)
            .GreaterThan(0).WithMessage("Fund ID must be greater than 0.");

        RuleFor(x => x.BudgetTypeId)
            .GreaterThan(0).WithMessage("Budget type ID must be greater than 0.");
    }
}
