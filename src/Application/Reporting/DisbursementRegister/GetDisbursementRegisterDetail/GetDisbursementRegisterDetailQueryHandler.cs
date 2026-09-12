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

        // Get accrual entry data
        int? accrualJournalEntryId = null;
        string? accrualEntryNumber = null;
        string? accrualEntryStatus = null;

        if (paymentOrder.AccrualJournalEntryId.HasValue)
        {
            var accrualEntry = await dbContext.JournalEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(je => je.Id == paymentOrder.AccrualJournalEntryId.Value, cancellationToken);

            if (accrualEntry is not null)
            {
                accrualJournalEntryId = accrualEntry.Id;
                accrualEntryNumber = accrualEntry.EntryNumber;
                accrualEntryStatus = accrualEntry.EntryStatus.ToString();
            }
        }

        return new DisbursementRegisterDetailDto
        {
            PaymentOrderId = paymentOrder.Id,
            OrderNumber = paymentOrder.PaymentOrderNumber,
            OrderDate = paymentOrder.PaymentOrderDate,
            PayeeName = paymentOrder.BeneficiaryName,
            Amount = paymentOrder.AmountGross,
            Status = paymentOrder.Status.ToString(),
            FundCode = fund.FundNumber,
            ApproverName = null,
            PaidAt = paymentOrder.PaidAt.HasValue ? DateOnly.FromDateTime(paymentOrder.PaidAt.Value.DateTime) : null,
            AccrualJournalEntryId = accrualJournalEntryId,
            AccrualEntryNumber = accrualEntryNumber,
            AccrualEntryStatus = accrualEntryStatus,
            Payments = payments
        };
    }
}
