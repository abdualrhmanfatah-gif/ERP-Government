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
    IApplicationDbContext context) : IRequestHandler<SubmitPaymentOrderCommand, Result>
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

        // Budget check: an open (non-Cancelled/Closed) budget must exist for the fund.
        // Computed availability enforcement for payments ships with the PostingPipeline
        // integration (out of scope); snapshot amounts no longer exist on Budget.
        var budget = await context.Budgets
            .FirstOrDefaultAsync(b => b.FundId == entity.FundId
                && b.Status != ERP_Government.Domain.Budgeting.Enums.BudgetStatus.Cancelled
                && b.Status != ERP_Government.Domain.Budgeting.Enums.BudgetStatus.Closed, cancellationToken);

        if (budget is null)
        {
            entity.BudgetCheckStatus = BudgetCheckStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure(["Budget check failed: No active budget found for this fund."]);
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
