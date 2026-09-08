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

            // Live aggregation from posted JournalEntryLines up to as-of date (DEP-026 — AccountBalances removed)
            var balances = await context.JournalEntryLines
                .Include(x => x.JournalEntry)
                .Include(x => x.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(x => x.JournalEntry.EntryStatus == EntryStatus.Posted
                         && x.JournalEntry.DocumentDate <= asOfDate
                         && x.Account.IsActive)
                .ToListAsync(cancellationToken);

            var assets = BuildGroup(balances, AccountGroupType.Asset);
            var liabilities = BuildGroup(balances, AccountGroupType.Liability);
            var equityBase = BuildGroup(balances, AccountGroupType.Equity);

            // Compute NetIncome from Revenue and Expense accounts
            var netIncome = balances
                .Where(x => x.Account.AccountGroup.Type == AccountGroupType.Revenue)
                .Sum(x => x.Credit - x.Debit)
                - balances
                .Where(x => x.Account.AccountGroup.Type == AccountGroupType.Expense)
                .Sum(x => x.Debit - x.Credit);

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
        List<JournalEntryLine> lines,
        AccountGroupType type)
    {
        var typeBalances = lines
            .Where(x => x.Account.AccountGroup.Type == type)
            .ToList();

        var grouped = typeBalances
            .GroupBy(x => x.Account.AccountGroup)
            .OrderBy(g => g.Key.Code)
            .Select(g => new ReportSection
            {
                Title = g.Key.Name,
                TitleEn = g.Key.Name,
                Lines = g.GroupBy(x => x.Account).Select(a => new ReportLine
                {
                    AccountCode = a.Key.Code,
                    AccountName = a.Key.Name,
                    Debit = a.Sum(x => x.Debit),
                    Credit = a.Sum(x => x.Credit),
                    Balance = type == AccountGroupType.Asset
                        ? a.Sum(x => x.Debit) - a.Sum(x => x.Credit)
                        : a.Sum(x => x.Credit) - a.Sum(x => x.Debit),
                }).ToList(),
                Total = g.GroupBy(x => x.Account).Sum(a => type == AccountGroupType.Asset
                    ? a.Sum(x => x.Debit) - a.Sum(x => x.Credit)
                    : a.Sum(x => x.Credit) - a.Sum(x => x.Debit)),
            })
            .ToList();

        return new BalanceSheetGroup
        {
            Sections = grouped,
            Total = grouped.Sum(s => s.Total),
        };
    }
}
