using FluentValidation;

namespace ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsReport;

internal class GetRevenueCollectionsReportQueryValidator : AbstractValidator<GetRevenueCollectionsReportQuery>
{
    public GetRevenueCollectionsReportQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("FiscalYearId is required.");
    }
}
