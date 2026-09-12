using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.TrialBalance.GetTrialBalanceReport;

internal class GetTrialBalanceReportQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTrialBalanceReportQuery, TrialBalanceReportDto>
{
    public async Task<TrialBalanceReportDto> Handle(
        GetTrialBalanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await dbContext.FiscalYears
            .AsNoTracking()
            .FirstAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        var yearStartDate = fiscalYear.StartDate;
        var yearEndDate = fiscalYear.EndDate;

        DateOnly periodEndDate = yearEndDate;
        string? periodName = null;

        if (request.FiscalPeriodId.HasValue)
        {
            var period = await dbContext.FiscalPeriods
                .AsNoTracking()
                .FirstAsync(p => p.Id == request.FiscalPeriodId.Value, cancellationToken);
            periodEndDate = period.EndDate;
            periodName = period.Name;
        }

        var query = dbContext.JournalEntryLines
            .AsNoTracking()
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
                .ThenInclude(a => a.AccountGroup)
            .Where(l => l.JournalEntry.FiscalYearId == request.FiscalYearId
                     && l.JournalEntry.EntryStatus == EntryStatus.Posted);

        var allLines = await query.ToListAsync(cancellationToken);

        // Lines within the reporting period (up to periodEndDate)
        var periodLines = allLines
            .Where(l => l.JournalEntry.DocumentDate <= periodEndDate);

        // Opening balance: posted Opening entries for these accounts in the SAME fiscal year
        var accountIdSet = allLines.Select(l => l.AccountId).Distinct().ToList();
        var openingQuery = dbContext.JournalEntryLines
            .AsNoTracking()
            .Include(l => l.JournalEntry)
            .Where(l => accountIdSet.Contains(l.AccountId)
                     && l.JournalEntry.EntryStatus == EntryStatus.Posted
                     && l.JournalEntry.EntryType == MoveEntryType.Opening
                     && l.JournalEntry.FiscalYearId == request.FiscalYearId);

        var openingLines = await openingQuery.ToListAsync(cancellationToken);

        var lineData = allLines
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var accountId = g.Key;
                var account = g.First().Account;
                var groupType = account.AccountGroup?.Type ?? AccountGroupType.Asset;

                var openingDebit = openingLines
                    .Where(l => l.AccountId == accountId)
                    .Sum(l => l.Debit);

                var openingCredit = openingLines
                    .Where(l => l.AccountId == accountId)
                    .Sum(l => l.Credit);

                var opening = openingDebit - openingCredit;

                var debitTotal = periodLines
                    .Where(l => l.AccountId == accountId)
                    .Sum(l => l.Debit);

                var creditTotal = periodLines
                    .Where(l => l.AccountId == accountId)
                    .Sum(l => l.Credit);

                var closing = opening + debitTotal - creditTotal;

                return new TrialBalanceLineDto
                {
                    AccountId = accountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = groupType.ToString(),
                    OpeningBalance = opening,
                    OpeningDebit = openingDebit,
                    OpeningCredit = openingCredit,
                    DebitTotal = debitTotal,
                    CreditTotal = creditTotal,
                    ClosingBalance = closing
                };
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        var totalDebits = lineData.Sum(l => l.DebitTotal);
        var totalCredits = lineData.Sum(l => l.CreditTotal);

        // Reconciliation check
        if (totalDebits != totalCredits)
        {
            throw new InvalidOperationException(
                $"Trial balance reconciliation failed: Total Debits ({totalDebits}) ≠ Total Credits ({totalCredits}).");
        }

        return new TrialBalanceReportDto
        {
            FiscalYearId = request.FiscalYearId,
            FiscalYearName = fiscalYear.Name,
            FiscalPeriodId = request.FiscalPeriodId,
            FiscalPeriodName = periodName,
            Lines = lineData,
            Totals = new TrialBalanceTotalDto
            {
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                TotalOpeningBalance = lineData.Sum(l => l.OpeningBalance),
                TotalClosingBalance = lineData.Sum(l => l.ClosingBalance)
            }
        };
    }
}
