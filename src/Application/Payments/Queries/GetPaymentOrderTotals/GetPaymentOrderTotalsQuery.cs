using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Queries.GetPaymentOrderTotals;

[Authorize(Policy = PermissionCodes.PaymentOrdersView)]
public class GetPaymentOrderTotalsQuery : IRequest<Result<PaymentOrderTotalsDto>>
{
    public int Id { get; init; }
}

public class GetPaymentOrderTotalsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPaymentOrderTotalsQuery, Result<PaymentOrderTotalsDto>>
{
    public async Task<Result<PaymentOrderTotalsDto>> Handle(
        GetPaymentOrderTotalsQuery request,
        CancellationToken cancellationToken)
    {
        var paymentOrder = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (paymentOrder is null)
            return Result<PaymentOrderTotalsDto>.Failure(ErrorCodes.Payments.PaymentOrderTotalsNotFound, ErrorCategory.NotFound, $"Payment order with ID {request.Id} not found.");

        var deductions = await context.PaymentOrderDeductions
            .Where(d => d.PaymentOrderId == request.Id)
            .ToListAsync(cancellationToken);

        var totalDeductions = deductions.Sum(d => d.Amount);
        var netAmount = paymentOrder.AmountGross - totalDeductions;

        var paidAmount = await context.Payments
            .Where(p => p.PaymentOrderId == request.Id && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);

        var remainingAmount = netAmount - paidAmount;

        return Result<PaymentOrderTotalsDto>.Success(new PaymentOrderTotalsDto
        {
            Id = paymentOrder.Id,
            AmountGross = paymentOrder.AmountGross,
            TotalDeductions = totalDeductions,
            NetAmount = netAmount,
            PaidAmount = paidAmount,
            RemainingAmount = remainingAmount,
            IsFullyPaid = remainingAmount <= 0m,
            Status = paymentOrder.Status
        });
    }
}

public class PaymentOrderTotalsDto
{
    public int Id { get; init; }
    public decimal AmountGross { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal NetAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public bool IsFullyPaid { get; init; }
    public PaymentOrderStatus Status { get; init; }
}
