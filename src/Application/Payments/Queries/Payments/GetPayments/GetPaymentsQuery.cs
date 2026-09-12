using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Queries.Payments.GetPayments;

public record GetPaymentsQuery(
    PaymentStatus? Status = null,
    int? FundId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null) : IRequest<List<PaymentDto>>;

public class GetPaymentsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPaymentsQuery, List<PaymentDto>>
{
    public async Task<List<PaymentDto>> Handle(
        GetPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Payments
            .Include(p => p.PaymentOrder)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.FundId.HasValue)
            query = query.Where(p => p.PaymentOrder.FundId == request.FundId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(p => p.PaidAt >= request.FromDate.Value.ToDateTime(TimeOnly.MinValue));

        if (request.ToDate.HasValue)
            query = query.Where(p => p.PaidAt <= request.ToDate.Value.ToDateTime(TimeOnly.MaxValue));

        return await query
            .OrderByDescending(p => p.PaidAt)
            .ThenByDescending(p => p.Id)
            .Select(p => new PaymentDto(
                p.Id,
                p.PaymentNumber,
                p.DisbursementRequestId,
                "",
                p.PaymentOrderId,
                p.PaymentOrder.PaymentOrderNumber,
                p.PaymentMethod,
                p.Amount,
                p.PaidById,
                p.PaidByName,
                p.PaidAt,
                p.ReferenceNumber,
                p.Notes,
                p.Status,
                p.PaymentOrder.BeneficiaryName))
            .ToListAsync(cancellationToken);
    }
}
