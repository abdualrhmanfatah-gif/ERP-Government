using ERP_Government.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsDetail;

internal class GetRevenueCollectionsDetailQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRevenueCollectionsDetailQuery, RevenueCollectionsDetailDto>
{
    public async Task<RevenueCollectionsDetailDto> Handle(
        GetRevenueCollectionsDetailQuery request,
        CancellationToken cancellationToken)
    {
        var voucher = await dbContext.ReceiptVouchers
            .AsNoTracking()
            .Include(rv => rv.Lines)
            .Include(rv => rv.Checks)
            .Include(rv => rv.Party)
            .Include(rv => rv.DepositSlip)
            .FirstAsync(rv => rv.Id == request.ReceiptVoucherId, cancellationToken);

        var accountIds = voucher.Lines.Select(l => l.RevenueAccountId).ToList();

        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => (a.Code, a.Name), cancellationToken);

        var lines = voucher.Lines
            .Select(l =>
            {
                var account = accounts.GetValueOrDefault(l.RevenueAccountId);
                return new RevenueCollectionsLineDetailDto
                {
                    RevenueAccountId = l.RevenueAccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Amount = l.Amount
                };
            })
            .ToList();

        var checks = voucher.Checks
            .Select(c => new CheckDetailDto
            {
                CheckId = c.Id,
                BankName = c.BankName,
                CheckNumber = c.CheckNumber,
                CheckDate = c.CheckDate,
                Amount = c.Amount,
                Status = c.Status.ToString(),
                ClearedAt = c.ClearedAt
            })
            .ToList();

        return new RevenueCollectionsDetailDto
        {
            ReceiptVoucherId = voucher.Id,
            VoucherNumber = voucher.VoucherNumber,
            VoucherDate = voucher.VoucherDate,
            PartyName = voucher.Party?.NameAr ?? string.Empty,
            TotalAmount = voucher.Lines.Sum(l => l.Amount),
            PaymentMethod = voucher.PaymentMethod.ToString(),
            DepositSlipNumber = voucher.DepositSlip?.SlipNumber,
            DepositSlipStatus = voucher.DepositSlip?.Status.ToString(),
            Lines = lines,
            Checks = checks
        };
    }
}
