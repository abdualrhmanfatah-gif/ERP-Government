using ERP_Government.Application.Common.Security;
using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.UpdatePurchaseOrder;

[Authorize(Policy = PermissionCodes.PurchaseOrdersCreate)]
public record UpdatePurchaseOrderCommand(
    int Id,
    string? PaymentTerms,
    string? DeliveryTerms,
    DateTime? ExpectedDeliveryDate,
    string? Notes,
    List<PurchaseOrders.CreatePurchaseOrder.PurchaseOrderLineDto> Lines) : IRequest<Result>;

public class UpdatePurchaseOrderCommandValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid purchase order ID.");

        RuleFor(x => x.Lines)
            .NotNull().WithMessage("Lines are required.")
            .NotEmpty().WithMessage("At least one line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.OrderedQuantity)
                .GreaterThan(0).WithMessage("Ordered quantity must be greater than 0.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be 0 or greater.");

            line.RuleFor(l => l.DiscountPercent)
                .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100.")
                .When(l => l.DiscountPercent.HasValue);

            line.RuleFor(l => l.TaxPercent)
                .InclusiveBetween(0, 100).WithMessage("Tax percent must be between 0 and 100.")
                .When(l => l.TaxPercent.HasValue);
        });
    }
}
