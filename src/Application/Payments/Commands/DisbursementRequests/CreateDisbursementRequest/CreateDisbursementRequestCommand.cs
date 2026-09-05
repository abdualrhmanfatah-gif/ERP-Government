using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsCreate)]
public class CreateDisbursementRequestCommand : IRequest<Result<DisbursementRequestDto>>
{
    public int PaymentOrderId { get; init; }
    public string? Notes { get; init; }
}

public class CreateDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IBudgetAvailabilityService availabilityService,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateDisbursementRequestCommand, Result<DisbursementRequestDto>>
{
    public async Task<Result<DisbursementRequestDto>> Handle(
        CreateDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<DisbursementRequestDto>.Failure(new[] { "User identity is required for this operation." });

        var paymentOrder = await context.PaymentOrders
            .FindAsync(request.PaymentOrderId, cancellationToken);

        if (paymentOrder is null)
            return Result<DisbursementRequestDto>.Failure(["Payment order not found."]);

        if (paymentOrder.Status != PaymentOrderStatus.Approved)
            return Result<DisbursementRequestDto>.Failure(["Payment order must be approved first."]);

        var netTotal = paymentOrder.AmountGross - paymentOrder.DeductionAmount;
        if (netTotal <= 0)
            return Result<DisbursementRequestDto>.Failure(["Payment order net total must be greater than zero."]);

        var existingRequest = await context.DisbursementRequests
            .FirstOrDefaultAsync(d => d.PaymentOrderId == request.PaymentOrderId, cancellationToken);

        if (existingRequest is not null)
            return Result<DisbursementRequestDto>.Failure(["A disbursement request already exists for this payment order."]);

        var requestNumber = await sequenceService.GenerateNextNumberAsync("DisbursementRequest", cancellationToken);
        if (requestNumber.StartsWith("Error:"))
            return Result<DisbursementRequestDto>.Failure([requestNumber]);

        var fiscalYearLapsed = await context.YearClosingRuns
            .AnyAsync(r => r.FiscalYearId == paymentOrder.FiscalYearId
                && r.Status == YearClosingRunStatus.Completed, cancellationToken);

        if (fiscalYearLapsed)
            return Result<DisbursementRequestDto>.Failure(
                ["Payments cannot be processed against a lapsed fiscal year. Reopen the fiscal year first."]);

        bool hasWarning = false;

        if (paymentOrder.AppropriationId > 0)
        {
            var appropriation = await context.Appropriations
                .FindAsync(paymentOrder.AppropriationId, cancellationToken);

            if (appropriation is not null)
            {
                var summary = await availabilityService.GetAvailabilitySummaryAsync(appropriation.BudgetItemId);
                var (allowed, warning) = availabilityService.EvaluateControlMethod(
                    summary.BudgetControlMethod, netTotal, summary.Available);

                if (!allowed)
                    return Result<DisbursementRequestDto>.Failure([
                        $"Budget availability insufficient. Net appropriated: {summary.NetAppropriated}, " +
                        $"Encumbered: {summary.Encumbered}, Available: {summary.Available}, " +
                        $"Requested: {netTotal}, Shortfall: {netTotal - summary.Available}"]);

                if (warning is not null)
                    hasWarning = true;
            }
        }

        var entity = new DisbursementRequest
        {
            RequestNumber = requestNumber,
            PaymentOrderId = request.PaymentOrderId,
            RequestedById = userId,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = DisbursementRequestStatus.Draft,
            HasWarning = hasWarning,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        context.DisbursementRequests.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DisbursementRequestDto>.Success(new DisbursementRequestDto(
            entity.Id,
            entity.RequestNumber,
            entity.PaymentOrderId,
            paymentOrder.PaymentOrderNumber,
            entity.RequestedById,
            "",
            entity.RequestDate,
            entity.Status,
            entity.HasWarning,
            entity.Notes,
            netTotal,
            paymentOrder.BeneficiaryName,
            null,
            null,
            null));
    }
}

public class CreateDisbursementRequestCommandValidator : AbstractValidator<CreateDisbursementRequestCommand>
{
    public CreateDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.PaymentOrderId)
            .GreaterThan(0).WithMessage("Payment order is required.");
    }
}
