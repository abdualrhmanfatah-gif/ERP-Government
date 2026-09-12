using ERP_Government.Application.Common.Security;
using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.SubmitPurchaseOrder;

[Authorize(Policy = PermissionCodes.PurchaseOrdersSubmit)]
public record SubmitPurchaseOrderCommand(int Id) : IRequest<Result>;

public class SubmitPurchaseOrderCommandValidator : AbstractValidator<SubmitPurchaseOrderCommand>
{
    public SubmitPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid purchase order ID.");
    }
}
