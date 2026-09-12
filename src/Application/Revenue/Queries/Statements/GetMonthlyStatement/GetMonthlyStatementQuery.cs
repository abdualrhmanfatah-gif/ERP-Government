using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.Statements.GetMonthlyStatement;

[Authorize(Policy = PermissionCodes.ReceiptVouchersView)]
public class GetMonthlyStatementQuery : IRequest<Result<MonthlyStatementDto>>
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int FundId { get; init; }
}

public class GetMonthlyStatementQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetMonthlyStatementQuery, Result<MonthlyStatementDto>>
{
    public async Task<Result<MonthlyStatementDto>> Handle(
        GetMonthlyStatementQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // FR-016: vouchers[] lists ALL approved vouchers of the month (full activity)
        var vouchers = await context.ReceiptVouchers
            .Include(v => v.Party)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .Where(v => v.VoucherDate >= startDate && v.VoucherDate <= endDate
                        && v.Status == ReceiptVoucherStatus.Approved)
            .ToListAsync(cancellationToken);

        var voucherIds = vouchers.Select(v => v.Id).ToList();

        var slips = await context.DepositSlips
            .Where(s => s.ReceiptVouchers.Any(v => voucherIds.Contains(v.Id)))
            .ToListAsync(cancellationToken);

        var approvedSlips = slips.Where(s => s.Status == DepositSlipStatus.Approved).ToList();

        var clearedChecks = await context.Checks
            .Where(c => voucherIds.Contains(c.ReceiptVoucherId) && c.Status == CheckStatus.Cleared)
            .ToListAsync(cancellationToken);

        var cashVouchers = vouchers.Where(v => v.PaymentMethod == PaymentMethod.Cash);
        var checkVouchers = vouchers.Where(v => v.PaymentMethod == PaymentMethod.Check);

        var summary = new MonthlyStatementSummaryDto
        {
            TotalCashCollections = cashVouchers.Sum(v => v.Lines.Sum(l => l.Amount)),
            TotalCheckCollections = checkVouchers.Sum(v => v.Lines.Sum(l => l.Amount)),
            TotalDeposited = approvedSlips.Sum(s => s.TotalAmount),
            TotalUnderCollection = checkVouchers.Where(v => v.Checks.Any(c => c.Status == CheckStatus.UnderCollection)).Sum(v => v.Lines.Sum(l => l.Amount)),
            TotalCleared = clearedChecks.Sum(c => c.Amount),
            TotalBounced = checkVouchers.Where(v => v.Checks.Any(c => c.Status == CheckStatus.Bounced)).Sum(v => v.Lines.Sum(l => l.Amount))
        };

        var statement = new MonthlyStatementDto
        {
            Year = request.Year,
            Month = request.Month,
            FundId = request.FundId,
            FundName = await GetFundNameAsync(request.FundId, cancellationToken),
            GeneratedAt = DateTimeOffset.UtcNow,
            Summary = summary,
            Vouchers = vouchers.Select(v => new ReceiptVoucherDto
            {
                Id = v.Id,
                VoucherNumber = v.VoucherNumber,
                VoucherDate = v.VoucherDate,
                PartyId = v.PartyId,
                PartyName = v.Party?.NameAr ?? string.Empty,
                PaymentMethod = v.PaymentMethod,
                PaymentMethodName = v.PaymentMethod.ToString(),
                ReceivedFrom = v.ReceivedFrom,
                Status = v.Status,
                StatusName = v.Status.ToString(),
                TotalAmount = v.Lines.Sum(l => l.Amount),
                RowVersion = v.RowVersion,
                DepositSlipNumber = slips.FirstOrDefault(s => s.ReceiptVouchers.Any(rv => rv.Id == v.Id))?.SlipNumber
            }).ToList(),
            Clearings = clearedChecks.Select(c => new CheckClearingDto
            {
                CheckNumber = c.CheckNumber,
                BankName = c.BankName,
                ClearedAt = c.ClearedAt ?? DateTimeOffset.MinValue,
                Amount = c.Amount
            }).ToList()
        };

        return Result<MonthlyStatementDto>.Success(statement);
    }

    private async Task<string> GetFundNameAsync(int fundId, CancellationToken cancellationToken)
    {
        var fund = await context.Funds.FindAsync(new object[] { fundId }, cancellationToken);
        return fund?.FundName ?? "Unknown Fund";
    }
}
