using FluentValidation;

namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;

internal class GetDisbursementRegisterQueryValidator : AbstractValidator<GetDisbursementRegisterQuery>
{
    public GetDisbursementRegisterQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("FiscalYearId is required.");
    }
}
