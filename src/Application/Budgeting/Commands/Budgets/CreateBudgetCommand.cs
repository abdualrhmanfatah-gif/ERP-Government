using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsCreate)]
public record CreateBudgetCommand(
    string BudgetName,
    int FiscalYearId,
    int FundId,
    int BudgetTypeId) : IRequest<Result<int>>;

public class CreateBudgetCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateBudgetCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateBudgetCommand request,
        CancellationToken cancellationToken)
    {
        var budgetNumber = await sequenceService.GenerateNextNumberAsync("Budget", cancellationToken);

        var fiscalYearExists = await context.FiscalYears
            .AnyAsync(x => x.Id == request.FiscalYearId, cancellationToken);

        if (!fiscalYearExists)
            return Result<int>.Failure(["Fiscal year not found."]);

        var fundExists = await context.Funds
            .AnyAsync(x => x.Id == request.FundId, cancellationToken);

        if (!fundExists)
            return Result<int>.Failure(["Fund not found."]);

        var budgetTypeExists = await context.BudgetTypes
            .AnyAsync(x => x.Id == request.BudgetTypeId, cancellationToken);

        if (!budgetTypeExists)
            return Result<int>.Failure(["Budget type not found."]);

        var entity = new Domain.Budgeting.Entities.Budget
        {
            BudgetNumber = budgetNumber,
            BudgetName = request.BudgetName,
            FiscalYearId = request.FiscalYearId,
            FundId = request.FundId,
            BudgetTypeId = request.BudgetTypeId,
            Status = BudgetStatus.Draft,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        context.Budgets.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
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
