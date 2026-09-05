using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Reports.BalanceSheet;

public class GetBalanceSheetQueryHandler(
    IApplicationDbContext context,
    ReportAuditService auditService) : IRequestHandler<GetBalanceSheetQuery, BalanceSheetDto>
{
    public async Task<BalanceSheetDto> Handle(
        GetBalanceSheetQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var asOfDate = DateOnly.TryParse(request.AsOfDate, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.Today);

            var balances = await context.AccountBalances
                .Include(x => x.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(x => x.Account.IsActive)
                .ToListAsync(cancellationToken);

            var assets = BuildGroup(balances, AccountGroupType.Asset, asOfDate);
            var liabilities = BuildGroup(balances, AccountGroupType.Liability, asOfDate);
            var equityBase = BuildGroup(balances, AccountGroupType.Equity, asOfDate);

            // Compute NetIncome from Revenue and Expense accounts
            var netIncome = balances
                .Where(x => x.Account.AccountGroup.Type == AccountGroupType.Revenue)
                .Sum(x => x.ClosingCredit - x.ClosingDebit)
                - balances
                .Where(x => x.Account.AccountGroup.Type == AccountGroupType.Expense)
                .Sum(x => x.ClosingDebit - x.ClosingCredit);

            var totalEquity = equityBase.Total + netIncome;

            // Add NetIncome as a line in the Equity section
            var equitySections = new List<ReportSection>(equityBase.Sections);
            if (netIncome != 0)
            {
                equitySections.Add(new ReportSection
                {
                    Title = "صافي الدخل / الخسارة",
                    TitleEn = "Net Income / Loss",
                    Lines =
                    [
                        new ReportLine
                        {
                            AccountCode = "NI",
                            AccountName = "صافي الدخل",
                            Balance = netIncome,
                        }
                    ],
                    Total = netIncome,
                });
            }

            var liabilitiesAndEquity = liabilities.Total + totalEquity;

            var result = new BalanceSheetDto
            {
                AsOfDate = asOfDate,
                Currency = "YER",
                Assets = assets,
                Liabilities = liabilities,
                Equity = new BalanceSheetGroup
                {
                    Sections = equitySections,
                    Total = totalEquity,
                },
                LiabilitiesAndEquity = liabilitiesAndEquity,
                Balanced = assets.Total == liabilitiesAndEquity,
                GeneratedAt = DateTimeOffset.UtcNow,
            };

            await auditService.LogAsync("BalanceSheet", request, "Screen", true, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await auditService.LogAsync("BalanceSheet", request, "Screen", false, ex.Message, cancellationToken);
            throw;
        }
    }

    private static BalanceSheetGroup BuildGroup(
        List<AccountBalance> balances,
        AccountGroupType type,
        DateOnly asOfDate)
    {
        var typeBalances = balances
            .Where(x => x.Account.AccountGroup.Type == type)
            .ToList();

        var grouped = typeBalances
            .GroupBy(x => x.Account.AccountGroup)
            .OrderBy(g => g.Key.Code)
            .Select(g => new ReportSection
            {
                Title = g.Key.Name,
                TitleEn = g.Key.Name,
                Lines = g.Select(x => new ReportLine
                {
                    AccountCode = x.Account.Code,
                    AccountName = x.Account.Name,
                    Debit = x.ClosingDebit,
                    Credit = x.ClosingCredit,
                    Balance = type == AccountGroupType.Asset
                        ? x.ClosingDebit - x.ClosingCredit
                        : x.ClosingCredit - x.ClosingDebit,
                }).ToList(),
                Total = g.Sum(x => type == AccountGroupType.Asset
                    ? x.ClosingDebit - x.ClosingCredit
                    : x.ClosingCredit - x.ClosingDebit),
            })
            .ToList();

        return new BalanceSheetGroup
        {
            Sections = grouped,
            Total = grouped.Sum(s => s.Total),
        };
    }
}
