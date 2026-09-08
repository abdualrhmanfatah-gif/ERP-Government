using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.SubmitDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsSubmit)]
public class SubmitDisbursementRequestCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class SubmitDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IBudgetAvailabilityService availabilityService,
    IUser user) : IRequestHandler<SubmitDisbursementRequestCommand, Result>
{
    public async Task<Result> Handle(
        SubmitDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.Draft)
            return Result.Failure(["Only draft disbursement requests can be submitted."]);

        var paymentOrder = await context.PaymentOrders
            .FindAsync(entity.PaymentOrderId, cancellationToken);

        if (paymentOrder is not null && paymentOrder.AppropriationId > 0)
        {
            var appropriation = await context.Appropriations
                .FindAsync(paymentOrder.AppropriationId, cancellationToken);

            if (appropriation is not null)
            {
                var netTotal = paymentOrder.AmountGross - paymentOrder.DeductionAmount;
                var summary = await availabilityService.GetAvailabilitySummaryAsync(appropriation.BudgetItemId);
                var (allowed, warning) = availabilityService.EvaluateControlMethod(
                    summary.BudgetControlMethod, netTotal, summary.Available);

                if (!allowed)
                    return Result.Failure([
                        $"Budget availability insufficient. Net appropriated: {summary.NetAppropriated}, " +
                        $"Encumbered: {summary.Encumbered}, Available: {summary.Available}, " +
                        $"Requested: {netTotal}, Shortfall: {netTotal - summary.Available}"]);

                if (warning is not null)
                    entity.HasWarning = true;
            }
        }

        entity.Status = DisbursementRequestStatus.PendingApproval;
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class SubmitDisbursementRequestCommandValidator : AbstractValidator<SubmitDisbursementRequestCommand>
{
    public SubmitDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");
    }
}
