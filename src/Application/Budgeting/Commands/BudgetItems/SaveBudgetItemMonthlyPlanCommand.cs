using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItems;

public record MonthlyPlanEntry(int FiscalPeriodId, decimal PlannedAmount);

[Authorize(Policy = PermissionCodes.BudgetItemsUpdate)]
public record SaveBudgetItemMonthlyPlanCommand(
    int BudgetItemId,
    List<MonthlyPlanEntry> Entries) : IRequest<Result>;

public class SaveBudgetItemMonthlyPlanCommandHandler(
    IApplicationDbContext context) : IRequestHandler<SaveBudgetItemMonthlyPlanCommand, Result>
{
    public async Task<Result> Handle(SaveBudgetItemMonthlyPlanCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.BudgetItemMonthlyPlans
            .Where(p => p.BudgetItemId == request.BudgetItemId)
            .ToListAsync(cancellationToken);

        context.BudgetItemMonthlyPlans.RemoveRange(existing);

        foreach (var entry in request.Entries)
        {
            context.BudgetItemMonthlyPlans.Add(new BudgetItemMonthlyPlan
            {
                BudgetItemId = request.BudgetItemId,
                FiscalPeriodId = entry.FiscalPeriodId,
                PlannedAmount = entry.PlannedAmount
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SaveBudgetItemMonthlyPlanCommandValidator : AbstractValidator<SaveBudgetItemMonthlyPlanCommand>
{
    public SaveBudgetItemMonthlyPlanCommandValidator()
    {
        RuleFor(x => x.BudgetItemId)
            .GreaterThan(0).WithMessage("Budget Item ID must be greater than 0.");

        RuleFor(x => x.Entries)
            .NotEmpty().WithMessage("At least one entry is required.");
        RuleFor(x => x.Entries)
            .Must(e => e.Count <= 12).WithMessage("Maximum 12 entries allowed.");

        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.FiscalPeriodId)
                .GreaterThan(0).WithMessage("Fiscal period ID must be greater than 0.");
            entry.RuleFor(e => e.PlannedAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Planned amount must be >= 0.");
        });

        RuleFor(x => x.Entries)
            .Must(e => e.Select(x => x.FiscalPeriodId).Distinct().Count() == e.Count)
            .WithMessage("Duplicate fiscal periods are not allowed.");
    }
}
