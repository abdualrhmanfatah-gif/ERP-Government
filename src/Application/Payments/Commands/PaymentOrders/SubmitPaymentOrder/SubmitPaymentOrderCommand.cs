using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.SubmitPaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersSubmit)]
public class SubmitPaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class SubmitPaymentOrderCommandHandler(
    IApplicationDbContext context,
    IBudgetAvailabilityService budgetAvailabilityService) : IRequestHandler<SubmitPaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        SubmitPaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Draft)
            return Result.Failure(["Only draft payment orders can be submitted."]);

        var hasLines = await context.PaymentOrderLines
            .AnyAsync(l => l.PaymentOrderId == entity.Id, cancellationToken);
        if (!hasLines)
            return Result.Failure(["Cannot submit a payment order with no lines."]);

        var appropriation = await context.Appropriations.FindAsync(entity.AppropriationId, cancellationToken);
        if (appropriation is null)
        {
            entity.BudgetCheckStatus = BudgetCheckStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure(["Budget check failed: Invalid appropriation."]);
        }

        var available = await budgetAvailabilityService.GetAvailableForAppropriationAsync(appropriation.BudgetItemId);
        var netAmount = entity.AmountGross - entity.DeductionAmount;

        if (available < netAmount)
        {
            entity.BudgetCheckStatus = BudgetCheckStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure([$"Budget check failed: Insufficient appropriation availability. Available: {available:N2}, Required: {netAmount:N2}."]);
        }

        entity.BudgetCheckStatus = BudgetCheckStatus.Passed;

        entity.Status = PaymentOrderStatus.Submitted;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class SubmitPaymentOrderCommandValidator : AbstractValidator<SubmitPaymentOrderCommand>
{
    public SubmitPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");
    }
}
