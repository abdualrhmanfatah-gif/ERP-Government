using FluentValidation;

namespace ERP_Government.Application.Reporting.TrialBalance.GetTrialBalanceReport;

internal class GetTrialBalanceReportQueryValidator : AbstractValidator<GetTrialBalanceReportQuery>
{
    public GetTrialBalanceReportQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("FiscalYearId is required.");
    }
}
