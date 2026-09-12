using ERP_Government.Application.Common.Security;
using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.CancelPurchaseOrder;

[Authorize(Policy = PermissionCodes.PurchaseOrdersCancel)]
public record CancelPurchaseOrderCommand(int Id, string? Reason = null) : IRequest<Result>;

public class CancelPurchaseOrderCommandValidator : AbstractValidator<CancelPurchaseOrderCommand>
{
    public CancelPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid purchase order ID.");
    }
}
