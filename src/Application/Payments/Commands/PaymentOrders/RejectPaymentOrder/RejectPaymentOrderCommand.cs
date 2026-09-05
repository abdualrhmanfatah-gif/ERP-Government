using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.RejectPaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersReject)]
public class RejectPaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string RejectionReason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class RejectPaymentOrderCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RejectPaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        RejectPaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Submitted)
            return Result.Failure(["Only submitted payment orders can be rejected."]);

        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            return Result.Failure(["Rejection reason is required."]);

        entity.Status = PaymentOrderStatus.Rejected;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RejectPaymentOrderCommandValidator : AbstractValidator<RejectPaymentOrderCommand>
{
    public RejectPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required.");
    }
}
