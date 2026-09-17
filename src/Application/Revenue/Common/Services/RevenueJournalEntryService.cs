using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Revenue.Common.Services;

public class RevenueJournalEntryService(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService)
{
    public async Task<JournalEntry> CreateSystemJournalEntryAsync(
        string narration,
        List<(int AccountId, decimal Debit, decimal Credit, string? Description)> lines,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var period = await context.FiscalPeriods
            .Where(p => p.IsActive && p.StartDate <= today && p.EndDate >= today)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await context.FiscalPeriods.FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active fiscal period found for today.");

        var entryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = today,
            PostingDate = today,
            EntryStatus = EntryStatus.Posted,
            EntryType = MoveEntryType.SystemGenerated,
            PeriodId = period.Id,
            FiscalYearId = period.FiscalYearId,
            IsSystemGenerated = true,
            Narration = narration,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);

        int seq = 1;
        foreach (var l in lines)
        {
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.Id,
                Sequence = seq++,
                AccountId = l.AccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description,
                CurrencyId = 1,
                ExchangeRate = 1
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return entry;
    }

    public async Task<int> GetOrCreateSystemAccountAsync(string code, string nameAr, NormalBalanceType balanceType, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Code == code, cancellationToken);

        if (account is not null)
            return account.Id;

        var defaultGroup = await context.AccountGroups.FirstOrDefaultAsync(cancellationToken);
        int groupId = defaultGroup?.Id ?? 1;

        account = new Account
        {
            Code = code,
            Name = nameAr,
            AccountGroupId = groupId,
            NormalBalance = balanceType,
            IsPostable = true,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);
        return account.Id;
    }
}
