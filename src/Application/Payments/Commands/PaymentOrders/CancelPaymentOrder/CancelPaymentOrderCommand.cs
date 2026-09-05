using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.CancelPaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersCancel)]
public class CancelPaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string CancellationReason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class CancelPaymentOrderCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CancelPaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        CancelPaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Draft && entity.Status != PaymentOrderStatus.Submitted)
            return Result.Failure(["Only draft or submitted payment orders can be cancelled."]);

        if (string.IsNullOrWhiteSpace(request.CancellationReason))
            return Result.Failure(["Cancellation reason is required."]);

        entity.Status = PaymentOrderStatus.Cancelled;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CancelPaymentOrderCommandValidator : AbstractValidator<CancelPaymentOrderCommand>
{
    public CancelPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("Cancellation reason is required.");
    }
}
