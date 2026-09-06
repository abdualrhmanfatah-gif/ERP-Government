using FluentValidation;

namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterDetail;

internal class GetDisbursementRegisterDetailQueryValidator : AbstractValidator<GetDisbursementRegisterDetailQuery>
{
    public GetDisbursementRegisterDetailQueryValidator()
    {
        RuleFor(x => x.PaymentOrderId)
            .GreaterThan(0).WithMessage("PaymentOrderId is required.");
    }
}
