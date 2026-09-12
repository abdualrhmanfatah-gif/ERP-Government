using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.SubmitPaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersSubmit)]
public class SubmitPaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int FundId { get; init; }
    public int? AccountId { get; init; }
    public int? BudgetItemAllocationId { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class SubmitPaymentOrderCommandHandler(
    IApplicationDbContext context,
    IBudgetAvailabilityService budgetAvailabilityService,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<SubmitPaymentOrderCommand, Result>
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

        // Validate that an accrual journal entry exists for the linked disbursement request
        if (entity.DisbursementRequestId.HasValue)
        {
            var disbursementRequest = await context.DisbursementRequests
                .FindAsync(entity.DisbursementRequestId.Value, cancellationToken);

            if (disbursementRequest?.AccrualJournalEntryId is null)
                return Result.Failure(["An accrual entry must be created before submitting the payment order."]);
        }

        // ADR-001 D-3/D-4: FundId required at submit
        entity.FundId = request.FundId;

        // Auto-fetch AccountId from BudgetItemAllocation → BudgetItem if not provided
        if (request.AccountId.HasValue)
        {
            entity.AccountId = request.AccountId.Value;
        }

        if (request.BudgetItemAllocationId.HasValue)
        {
            var allocation = await context.BudgetItemAllocations
                .Include(a => a.BudgetItem)
                .Include(a => a.Budget)
                .FirstOrDefaultAsync(a => a.Id == request.BudgetItemAllocationId.Value, cancellationToken);

            if (allocation is null)
                return Result.Failure(["Budget item allocation not found."]);

            if (allocation.Budget.Status != Domain.Budgeting.Enums.BudgetStatus.Active)
                return Result.Failure(["Budget must be active to submit payment order."]);

            entity.BudgetItemAllocationId = request.BudgetItemAllocationId.Value;

            // Auto-fetch AccountId from BudgetItem if not set
            if (!entity.AccountId.HasValue && allocation.BudgetItem?.AccountId.HasValue == true)
            {
                entity.AccountId = allocation.BudgetItem.AccountId.Value;
            }

            // Budget check against BudgetItemAllocation
            var available = await budgetAvailabilityService.GetAvailableForAppropriationAsync(allocation.Id);
            var netAmount = entity.AmountGross - entity.DeductionAmount;

            if (available < netAmount)
            {
                await context.SaveChangesAsync(cancellationToken);
                return Result.Failure([$"Budget check failed: Insufficient budget availability. Available: {available:N2}, Required: {netAmount:N2}."]);
            }
        }

        entity.Status = PaymentOrderStatus.Submitted;

        if (user.Id is int userId)
        {
            await statusLogger.LogAsync(
                "paymentorders",
                entity.Id,
                PaymentOrderStatus.Draft.ToString(),
                PaymentOrderStatus.Submitted.ToString(),
                userId,
                null,
                cancellationToken);
        }

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

        RuleFor(x => x.FundId)
            .GreaterThan(0).WithMessage("Fund is required at submit.");
    }
}
