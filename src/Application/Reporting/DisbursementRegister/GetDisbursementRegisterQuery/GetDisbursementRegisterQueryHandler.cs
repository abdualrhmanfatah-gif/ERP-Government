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

        var funds = await dbContext.Funds
            .AsNoTracking()
            .Where(f => fundIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => (f.FundNumber, f.FundName), cancellationToken);

        // Get disbursement requests linked to payment orders
        var disbursementRequestIds = paymentOrders
            .Where(po => po.DisbursementRequestId.HasValue)
            .Select(po => po.DisbursementRequestId!.Value)
            .Distinct()
            .ToList();

        var disbursementRequests = await dbContext.DisbursementRequests
            .AsNoTracking()
            .Where(dr => disbursementRequestIds.Contains(dr.Id))
            .ToDictionaryAsync(dr => dr.Id, cancellationToken);

        // Get accrual journal entries
        var accrualJournalEntryIds = disbursementRequests.Values
            .Where(dr => dr.AccrualJournalEntryId.HasValue)
            .Select(dr => dr.AccrualJournalEntryId!.Value)
            .Distinct()
            .ToList();

        var accrualJournalEntries = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(je => accrualJournalEntryIds.Contains(je.Id))
            .ToDictionaryAsync(je => je.Id, cancellationToken);

        var lines = paymentOrders
            .Select(po =>
            {
                var fund = funds.GetValueOrDefault(po.FundId);

                int? accrualJournalEntryId = null;
                string? accrualEntryNumber = null;
                string? accrualEntryStatus = null;

                if (po.DisbursementRequestId.HasValue
                    && disbursementRequests.TryGetValue(po.DisbursementRequestId.Value, out var dr)
                    && dr.AccrualJournalEntryId.HasValue
                    && accrualJournalEntries.TryGetValue(dr.AccrualJournalEntryId.Value, out var accrualEntry))
                {
                    accrualJournalEntryId = accrualEntry.Id;
                    accrualEntryNumber = accrualEntry.EntryNumber;
                    accrualEntryStatus = accrualEntry.EntryStatus.ToString();
                }

                return new DisbursementRegisterLineDto
                {
                    PaymentOrderId = po.Id,
                    OrderNumber = po.PaymentOrderNumber,
                    OrderDate = po.PaymentOrderDate,
                    PayeeName = po.BeneficiaryName,
                    Amount = po.AmountGross,
                    Status = po.Status.ToString(),
                    FundId = po.FundId,
                    FundCode = fund.FundNumber,
                    FundName = fund.FundName,
                    ApproverId = null,
                    ApproverName = null,
                    PaidAt = po.PaidAt.HasValue ? DateOnly.FromDateTime(po.PaidAt.Value.DateTime) : null,
                    AccrualJournalEntryId = accrualJournalEntryId,
                    AccrualEntryNumber = accrualEntryNumber,
                    AccrualEntryStatus = accrualEntryStatus
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
