using FluentValidation;

namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotDetail;

internal class GetAvailabilitySnapshotDetailQueryValidator : AbstractValidator<GetAvailabilitySnapshotDetailQuery>
{
    public GetAvailabilitySnapshotDetailQueryValidator()
    {
        RuleFor(x => x.BudgetItemId)
            .GreaterThan(0).WithMessage("BudgetItemId is required.");

        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("FiscalYearId is required.");
    }
}
