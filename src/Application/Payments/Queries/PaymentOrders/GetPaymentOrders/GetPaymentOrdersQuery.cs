using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrders;

[Authorize(Policy = PermissionCodes.PaymentOrdersView)]
public class GetPaymentOrdersQuery : IRequest<List<PaymentOrderDto>>
{
    public PaymentOrderStatus? Status { get; init; }
    public int? VendorId { get; init; }
    public int? FundId { get; init; }
}

public class GetPaymentOrdersQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPaymentOrdersQuery, List<PaymentOrderDto>>
{
    public async Task<List<PaymentOrderDto>> Handle(
        GetPaymentOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.PaymentOrders.AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.VendorId.HasValue)
            query = query.Where(x => x.VendorId == request.VendorId.Value);

        if (request.FundId.HasValue)
            query = query.Where(x => x.FundId == request.FundId.Value);

        return await query
            .OrderByDescending(x => x.PaymentOrderDate)
            .Select(x => new PaymentOrderDto
            {
                Id = x.Id,
                PaymentOrderNumber = x.PaymentOrderNumber,
                PaymentOrderDate = x.PaymentOrderDate.ToDateTime(TimeOnly.MinValue),
                DueDate = x.DueDate.HasValue ? x.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                PaymentOrderType = x.PaymentOrderType,
                VendorId = x.VendorId,
                FundId = x.FundId,
                FiscalYearId = x.FiscalYearId,
                AppropriationId = x.AppropriationId,
                CurrencyId = x.CurrencyId,
                AmountGross = x.AmountGross,
                DeductionAmount = x.DeductionAmount,
                PaymentMethod = x.PaymentMethod,
                BeneficiaryName = x.BeneficiaryName,
                Status = x.Status,
                BudgetCheckStatus = x.BudgetCheckStatus,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }
}
