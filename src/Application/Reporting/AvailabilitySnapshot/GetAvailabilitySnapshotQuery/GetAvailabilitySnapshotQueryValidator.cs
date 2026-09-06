using FluentValidation;

namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotQuery;

internal class GetAvailabilitySnapshotQueryValidator : AbstractValidator<GetAvailabilitySnapshotQuery>
{
    public GetAvailabilitySnapshotQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("FiscalYearId is required.");

        RuleFor(x => x.BudgetItemId)
            .GreaterThan(0).When(x => x.BudgetItemId.HasValue)
            .WithMessage("BudgetItemId must be greater than 0 when specified.");
    }
}
