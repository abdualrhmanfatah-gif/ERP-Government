using FluentValidation;

namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;

internal class GetBudgetExecutionReportQueryValidator : AbstractValidator<GetBudgetExecutionReportQuery>
{
    public GetBudgetExecutionReportQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("FiscalYearId is required.");
    }
}
