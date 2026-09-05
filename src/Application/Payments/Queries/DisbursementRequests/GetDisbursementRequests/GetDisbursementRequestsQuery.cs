using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequests;

public record GetDisbursementRequestsQuery(
    DisbursementRequestStatus? Status = null,
    int? FundId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int? PaymentOrderId = null) : IRequest<List<DisbursementRequestDto>>;

public class GetDisbursementRequestsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDisbursementRequestsQuery, List<DisbursementRequestDto>>
{
    public async Task<List<DisbursementRequestDto>> Handle(
        GetDisbursementRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.DisbursementRequests
            .Include(d => d.PaymentOrder)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(d => d.Status == request.Status.Value);

        if (request.FundId.HasValue)
            query = query.Where(d => d.PaymentOrder.FundId == request.FundId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(d => d.RequestDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(d => d.RequestDate <= request.ToDate.Value);

        if (request.PaymentOrderId.HasValue)
            query = query.Where(d => d.PaymentOrderId == request.PaymentOrderId.Value);

        return await query
            .OrderByDescending(d => d.RequestDate)
            .ThenByDescending(d => d.Id)
            .Select(d => new DisbursementRequestDto(
                d.Id,
                d.RequestNumber,
                d.PaymentOrderId,
                d.PaymentOrder.PaymentOrderNumber,
                d.RequestedById,
                "",
                d.RequestDate,
                d.Status,
                d.HasWarning,
                d.Notes,
                d.PaymentOrder.AmountGross - d.PaymentOrder.DeductionAmount,
                d.PaymentOrder.BeneficiaryName,
                null,
                null,
                null))
            .ToListAsync(cancellationToken);
    }
}
