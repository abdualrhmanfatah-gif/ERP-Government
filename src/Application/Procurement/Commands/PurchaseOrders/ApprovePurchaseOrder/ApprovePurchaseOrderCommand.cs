using ERP_Government.Application.Common.Security;
using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.ApprovePurchaseOrder;

[Authorize(Policy = PermissionCodes.PurchaseOrdersApprove)]
public record ApprovePurchaseOrderCommand(int Id) : IRequest<Result>;

public class ApprovePurchaseOrderCommandValidator : AbstractValidator<ApprovePurchaseOrderCommand>
{
    public ApprovePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid purchase order ID.");
    }
}
