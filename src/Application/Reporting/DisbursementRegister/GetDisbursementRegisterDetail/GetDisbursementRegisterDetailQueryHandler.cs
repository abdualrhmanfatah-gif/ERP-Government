using ERP_Government.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterDetail;

internal class GetDisbursementRegisterDetailQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetDisbursementRegisterDetailQuery, DisbursementRegisterDetailDto>
{
    public async Task<DisbursementRegisterDetailDto> Handle(
        GetDisbursementRegisterDetailQuery request,
        CancellationToken cancellationToken)
    {
        var paymentOrder = await dbContext.PaymentOrders
            .AsNoTracking()
            .FirstAsync(po => po.Id == request.PaymentOrderId, cancellationToken);

        var vendor = await dbContext.Parties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentOrder.VendorId, cancellationToken);

        var fund = await dbContext.Funds
            .AsNoTracking()
            .FirstAsync(f => f.Id == paymentOrder.FundId, cancellationToken);

        var payments = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.PaymentOrderId == request.PaymentOrderId)
            .Select(p => new PaymentDetailDto
            {
                PaymentId = p.Id,
                PaymentNumber = p.PaymentNumber,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                PaidAt = p.PaidAt,
                Status = p.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new DisbursementRegisterDetailDto
        {
            PaymentOrderId = paymentOrder.Id,
            OrderNumber = paymentOrder.PaymentOrderNumber,
            OrderDate = paymentOrder.PaymentOrderDate,
            PayeeName = vendor?.NameEn ?? vendor?.NameAr ?? paymentOrder.BeneficiaryName,
            Amount = paymentOrder.AmountGross,
            Status = paymentOrder.Status.ToString(),
            FundCode = fund.FundNumber,
            ApproverName = null,
            PaidAt = paymentOrder.PaidAt.HasValue ? DateOnly.FromDateTime(paymentOrder.PaidAt.Value.DateTime) : null,
            Payments = payments
        };
    }
}
