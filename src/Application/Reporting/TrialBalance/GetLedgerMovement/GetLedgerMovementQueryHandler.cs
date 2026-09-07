using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.TrialBalance.GetLedgerMovement;

internal class GetLedgerMovementQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLedgerMovementQuery, LedgerMovementDto>
{
    public async Task<LedgerMovementDto> Handle(
        GetLedgerMovementQuery request,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstAsync(a => a.Id == request.AccountId, cancellationToken);

        var fiscalYear = await dbContext.FiscalYears
            .AsNoTracking()
            .FirstAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        var query = dbContext.JournalEntryLines
            .AsNoTracking()
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == request.AccountId
                     && l.JournalEntry.FiscalYearId == request.FiscalYearId
                     && l.JournalEntry.EntryStatus == EntryStatus.Posted);

        var lines = await query
            .OrderBy(l => l.JournalEntry.DocumentDate)
            .ThenBy(l => l.JournalEntry.EntryNumber)
            .ToListAsync(cancellationToken);

        // Opening balance: all posted entries for this account before fiscal year start
        var openingBalance = await dbContext.JournalEntryLines
            .AsNoTracking()
            .Where(l => l.AccountId == request.AccountId
                     && l.JournalEntry.EntryStatus == EntryStatus.Posted
                     && l.JournalEntry.DocumentDate < fiscalYear.StartDate)
            .SumAsync(l => l.Debit - l.Credit, cancellationToken);

        var runningBalance = openingBalance;

        var entries = lines.Select(l =>
        {
            runningBalance += l.Debit - l.Credit;

            return new LedgerMovementLineDto
            {
                JournalEntryId = l.JournalEntryId,
                EntryNumber = l.JournalEntry.EntryNumber,
                EntryDate = l.JournalEntry.DocumentDate,
                Description = l.Description ?? l.JournalEntry.Narration,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = runningBalance
            };
        }).ToList();

        var totalDebits = entries.Sum(e => e.Debit);
        var totalCredits = entries.Sum(e => e.Credit);

        return new LedgerMovementDto
        {
            AccountId = request.AccountId,
            AccountCode = account.Code,
            AccountName = account.Name,
            Entries = entries,
            Totals = new LedgerMovementTotalDto
            {
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                FinalBalance = runningBalance
            }
        };
    }
}
