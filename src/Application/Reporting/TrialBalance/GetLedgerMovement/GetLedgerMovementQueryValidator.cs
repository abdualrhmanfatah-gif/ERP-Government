using FluentValidation;

namespace ERP_Government.Application.Reporting.TrialBalance.GetLedgerMovement;

internal class GetLedgerMovementQueryValidator : AbstractValidator<GetLedgerMovementQuery>
{
    public GetLedgerMovementQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("AccountId is required.");

        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("FiscalYearId is required.");
    }
}
