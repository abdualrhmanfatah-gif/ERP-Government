using ERP_Government.Application.Common.Interfaces;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;

public class CreatePurchaseRequestCommandValidator : AbstractValidator<CreatePurchaseRequestCommand>
{
    public CreatePurchaseRequestCommandValidator()
    {
        RuleFor(x => x.RequestDate)
            .NotEmpty().WithMessage("Request date is required.");

        RuleFor(x => x.RequesterName)
            .NotEmpty().WithMessage("Requester name is required.")
            .MaximumLength(200).WithMessage("Requester name must not exceed 200 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId)
                .GreaterThan(0).WithMessage("Item is required.");

            line.RuleFor(l => l.UnitId)
                .GreaterThan(0).WithMessage("Unit is required.");

            line.RuleFor(l => l.RequestedQuantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
