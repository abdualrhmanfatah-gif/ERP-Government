using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Revenue.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsReport;

internal class GetRevenueCollectionsReportQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRevenueCollectionsReportQuery, RevenueCollectionsReportDto>
{
    public async Task<RevenueCollectionsReportDto> Handle(
        GetRevenueCollectionsReportQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await dbContext.FiscalYears
            .AsNoTracking()
            .FirstAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        var query = dbContext.ReceiptVouchers
            .AsNoTracking()
            .Include(rv => rv.Lines)
            .Include(rv => rv.Checks)
            .Include(rv => rv.Party)
            .Include(rv => rv.DepositSlip47)
            .Where(rv => rv.Status != ReceiptVoucherStatus.Cancelled);

        if (request.RevenueAccountId.HasValue)
            query = query.Where(rv => rv.Lines.Any(l => l.RevenueAccountId == request.RevenueAccountId.Value));

        if (request.PartyId.HasValue)
            query = query.Where(rv => rv.PartyId == request.PartyId.Value);

        if (!string.IsNullOrEmpty(request.PaymentMethod)
            && Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var method))
        {
            query = query.Where(rv => rv.PaymentMethod == method);
        }

        var vouchers = await query.ToListAsync(cancellationToken);

        var accountIds = vouchers.SelectMany(rv => rv.Lines)
            .Select(l => l.RevenueAccountId)
            .Distinct()
            .ToList();

        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => (a.Code, a.Name), cancellationToken);

        var lines = vouchers
            .SelectMany(rv => rv.Lines.Select(l => new { rv, l }))
            .Select(x =>
            {
                var account = accounts.GetValueOrDefault(x.l.RevenueAccountId);
                var checkStatus = GetCheckClearingStatus(x.rv);
                var slipStatus = x.rv.DepositSlip47?.ApprovedAt.HasValue == true ? "Approved" : "Draft";

                return new RevenueCollectionsLineDto
                {
                    ReceiptVoucherId = x.rv.Id,
                    VoucherNumber = x.rv.VoucherNumber,
                    VoucherDate = x.rv.VoucherDate,
                    RevenueAccountId = x.l.RevenueAccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    PartyId = x.rv.PartyId,
                    PartyName = x.rv.Party?.NameAr ?? string.Empty,
                    Amount = x.l.Amount,
                    PaymentMethod = x.rv.PaymentMethod.ToString(),
                    DepositSlipId = x.rv.DepositSlip47Id,
                    DepositSlipNumber = x.rv.DepositSlip47?.SlipNumber,
                    DepositSlipStatus = slipStatus,
                    CheckClearingStatus = checkStatus
                };
            })
            .OrderBy(l => l.VoucherDate)
            .ThenBy(l => l.VoucherNumber)
            .ToList();

        var allChecks = vouchers.SelectMany(rv => rv.Checks).ToList();

        return new RevenueCollectionsReportDto
        {
            FiscalYearId = request.FiscalYearId,
            FiscalYearName = fiscalYear.Name,
            Lines = lines,
            Totals = new RevenueCollectionsTotalDto
            {
                TotalAmount = lines.Sum(l => l.Amount),
                TotalCash = lines.Where(l => l.PaymentMethod == nameof(PaymentMethod.Cash)).Sum(l => l.Amount),
                TotalChecks = lines.Where(l => l.PaymentMethod == nameof(PaymentMethod.Check)).Sum(l => l.Amount),
                PendingDeposits = vouchers.Count(rv => rv.DepositSlip47Id == null),
                ClearedChecks = allChecks.Count(c => c.Status == CheckStatus.Cleared),
                BouncedChecks = allChecks.Count(c => c.Status == CheckStatus.Bounced)
            }
        };
    }

    private static string GetCheckClearingStatus(Domain.Revenue.Entities.ReceiptVoucher rv)
    {
        if (rv.PaymentMethod != PaymentMethod.Check)
            return string.Empty;

        if (rv.Checks.Count == 0)
            return string.Empty;

        if (rv.Checks.All(c => c.Status == CheckStatus.Cleared))
            return nameof(CheckStatus.Cleared);

        if (rv.Checks.Any(c => c.Status == CheckStatus.Bounced))
            return nameof(CheckStatus.Bounced);

        return nameof(CheckStatus.UnderCollection);
    }
}
