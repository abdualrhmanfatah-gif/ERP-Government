using ERP_Government.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;

internal class GetDisbursementRegisterQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetDisbursementRegisterQuery, DisbursementRegisterDto>
{
    public async Task<DisbursementRegisterDto> Handle(
        GetDisbursementRegisterQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await dbContext.FiscalYears
            .AsNoTracking()
            .FirstAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        var query = dbContext.PaymentOrders
            .AsNoTracking()
            .Where(po => po.FiscalYearId == request.FiscalYearId
                      && po.Status != Domain.Payments.Enums.PaymentOrderStatus.Cancelled);

        if (request.FundId.HasValue)
            query = query.Where(po => po.FundId == request.FundId.Value);

        if (!string.IsNullOrEmpty(request.Status)
            && Enum.TryParse<Domain.Payments.Enums.PaymentOrderStatus>(request.Status, true, out var statusEnum))
        {
            query = query.Where(po => po.Status == statusEnum);
        }

        if (request.ApproverId.HasValue)
            query = query.Where(po => po.CreatedBy != null && po.LastModifiedBy != null);

        var paymentOrders = await query.ToListAsync(cancellationToken);

        var fundIds = paymentOrders.Select(po => po.FundId).Distinct().ToList();
        var vendorIds = paymentOrders.Select(po => po.VendorId).Distinct().ToList();

        var funds = await dbContext.Funds
            .AsNoTracking()
            .Where(f => fundIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => (f.FundNumber, f.FundName), cancellationToken);

        var vendors = await dbContext.Parties
            .AsNoTracking()
            .Where(p => vendorIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.NameEn ?? p.NameAr, cancellationToken);

        var lines = paymentOrders
            .Select(po =>
            {
                var fund = funds.GetValueOrDefault(po.FundId);
                var payee = vendors.GetValueOrDefault(po.VendorId);

                return new DisbursementRegisterLineDto
                {
                    PaymentOrderId = po.Id,
                    OrderNumber = po.PaymentOrderNumber,
                    OrderDate = po.PaymentOrderDate,
                    PayeeName = payee ?? po.BeneficiaryName,
                    Amount = po.AmountGross,
                    Status = po.Status.ToString(),
                    FundId = po.FundId,
                    FundCode = fund.FundNumber,
                    FundName = fund.FundName,
                    ApproverId = null,
                    ApproverName = null,
                    PaidAt = po.PaidAt.HasValue ? DateOnly.FromDateTime(po.PaidAt.Value.DateTime) : null
                };
            })
            .OrderBy(l => l.OrderDate)
            .ThenBy(l => l.OrderNumber)
            .ToList();

        return new DisbursementRegisterDto
        {
            FiscalYearId = request.FiscalYearId,
            FiscalYearName = fiscalYear.Name,
            Lines = lines,
            Totals = new DisbursementRegisterTotalDto
            {
                TotalRequests = lines.Count,
                DraftCount = lines.Count(l => l.Status == Domain.Payments.Enums.PaymentOrderStatus.Draft.ToString()),
                SubmittedCount = lines.Count(l => l.Status == Domain.Payments.Enums.PaymentOrderStatus.Submitted.ToString()),
                ApprovedCount = lines.Count(l => l.Status == Domain.Payments.Enums.PaymentOrderStatus.Approved.ToString()),
                PaidCount = lines.Count(l => l.Status == Domain.Payments.Enums.PaymentOrderStatus.Paid.ToString()),
                RejectedCount = lines.Count(l => l.Status == Domain.Payments.Enums.PaymentOrderStatus.Rejected.ToString()),
                TotalAmount = lines.Sum(l => l.Amount),
                PaidAmount = lines.Where(l => l.Status == Domain.Payments.Enums.PaymentOrderStatus.Paid.ToString()).Sum(l => l.Amount)
            }
        };
    }
}
