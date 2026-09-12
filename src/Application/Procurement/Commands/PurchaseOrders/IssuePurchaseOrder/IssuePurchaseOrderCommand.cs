using ERP_Government.Application.Common.Security;
using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.IssuePurchaseOrder;

[Authorize(Policy = PermissionCodes.PurchaseOrdersIssue)]
public record IssuePurchaseOrderCommand(int Id) : IRequest<Result>;

public class IssuePurchaseOrderCommandValidator : AbstractValidator<IssuePurchaseOrderCommand>
{
    public IssuePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid purchase order ID.");
    }
}
