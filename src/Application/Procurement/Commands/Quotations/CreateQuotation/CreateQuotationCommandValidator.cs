using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation;

public class CreateQuotationCommandValidator : AbstractValidator<CreateQuotationCommand>
{
    public CreateQuotationCommandValidator()
    {
        RuleFor(x => x.SupplierPartyId)
            .GreaterThan(0).WithMessage("Supplier is required.");

        RuleFor(x => x.QuotationDate)
            .NotEmpty().WithMessage("Quotation date is required.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId)
                .GreaterThan(0).WithMessage("Item is required.");

            line.RuleFor(l => l.UnitId)
                .GreaterThan(0).WithMessage("Unit is required.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to zero.");

            line.RuleFor(l => l.DiscountPercent)
                .InclusiveBetween(0, 100).When(l => l.DiscountPercent.HasValue)
                .WithMessage("Discount percent must be between 0 and 100.");

            line.RuleFor(l => l.TaxPercent)
                .InclusiveBetween(0, 100).When(l => l.TaxPercent.HasValue)
                .WithMessage("Tax percent must be between 0 and 100.");
        });
    }
}
