using ERP_Government.Application.Common.Security;
using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.ClosePurchaseOrder;

[Authorize(Policy = PermissionCodes.PurchaseOrdersClose)]
public record ClosePurchaseOrderCommand(int Id, string? Reason = null) : IRequest<Result>;

public class ClosePurchaseOrderCommandValidator : AbstractValidator<ClosePurchaseOrderCommand>
{
    public ClosePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid purchase order ID.");
    }
}
