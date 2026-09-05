using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.VoidPaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersVoid)]
public class VoidPaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string VoidReason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class VoidPaymentOrderCommandHandler(
    IApplicationDbContext context) : IRequestHandler<VoidPaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        VoidPaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Paid)
            return Result.Failure(["Only paid payment orders can be voided."]);

        if (string.IsNullOrWhiteSpace(request.VoidReason))
            return Result.Failure(["Void reason is required."]);

        entity.Status = PaymentOrderStatus.Voided;

        // TODO: Create reversing accounting entry

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class VoidPaymentOrderCommandValidator : AbstractValidator<VoidPaymentOrderCommand>
{
    public VoidPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");

        RuleFor(x => x.VoidReason)
            .NotEmpty().WithMessage("Void reason is required.");
    }
}
