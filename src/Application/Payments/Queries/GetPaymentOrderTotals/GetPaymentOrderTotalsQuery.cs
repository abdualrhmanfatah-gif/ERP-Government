using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Queries.GetPaymentOrderTotals;

[Authorize(Policy = PermissionCodes.PaymentOrdersView)]
public class GetPaymentOrderTotalsQuery : IRequest<PaymentOrderTotalsDto?>
{
    public int Id { get; init; }
}

public class GetPaymentOrderTotalsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPaymentOrderTotalsQuery, PaymentOrderTotalsDto?>
{
    public async Task<PaymentOrderTotalsDto?> Handle(
        GetPaymentOrderTotalsQuery request,
        CancellationToken cancellationToken)
    {
        var paymentOrder = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (paymentOrder is null)
            return null;

        var deductions = await context.PaymentOrderDeductions
            .Where(d => d.PaymentOrderId == request.Id)
            .ToListAsync(cancellationToken);

        var totalDeductions = deductions.Sum(d => d.Amount);
        var netAmount = paymentOrder.AmountGross - totalDeductions;

        var isPaid = paymentOrder.Status == PaymentOrderStatus.Paid;
        var isPartiallyPaid = paymentOrder.Status == PaymentOrderStatus.PartiallyPaid;
        var paidAmount = isPaid ? netAmount : 0m;
        var remainingAmount = netAmount - paidAmount;

        return new PaymentOrderTotalsDto
        {
            Id = paymentOrder.Id,
            AmountGross = paymentOrder.AmountGross,
            TotalDeductions = totalDeductions,
            NetAmount = netAmount,
            PaidAmount = paidAmount,
            RemainingAmount = remainingAmount,
            IsFullyPaid = isPaid,
            Status = paymentOrder.Status
        };
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
