using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;

namespace ERP_Government.Application.Accounting.Integration.Assets;

public class DepreciationPostedHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : INotificationHandler<DepreciationPosted>
{
    public async Task Handle(DepreciationPosted notification, CancellationToken ct)
    {
        var run = await context.DepreciationRuns
            .Include(r => r.ScheduleLines)
                .ThenInclude(s => s.Asset)
                    .ThenInclude(a => a!.AssetGroup)
            .FirstOrDefaultAsync(r => r.Id == notification.SourceEntityId, ct);
        if (run is null || run.Status == DepreciationRunStatus.Posted) return;

        var entry = await FindExistingEntryAsync(run.Id, run.JournalEntryId, ct);
        if (entry is not null) return;

        entry = new JournalEntry
        {
            EntryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", ct),
            Ref = run.RunNumber,
            DocumentDate = run.DepreciationDate,
            PostingDate = DateOnly.FromDateTime(DateTime.Today),
            EntryStatus = EntryStatus.Draft,
                EntryType = MoveEntryType.Accrual,
            PeriodId = notification.PeriodId,
            FiscalYearId = run.FiscalYearId,
            IsSystemGenerated = true,
            Narration = $"إهلاك الأصول - العملية {run.RunNumber}",
            CreatedBy = "System"
        };
        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync(ct);

        var sequence = 1;
        foreach (var schedule in run.ScheduleLines.Where(s => s.Amount > 0))
        {
            var debitAccountId = schedule.Asset!.AssetGroup!.DepreciationExpenseAccountId;
            var creditAccountId = schedule.Asset!.AssetGroup!.AccumulatedDepreciationAccountId;
            if (debitAccountId is null || creditAccountId is null) return;

            var currencyId = schedule.Asset!.CurrencyId;
            var amount = Math.Round(schedule.Amount, 2);
            var description = $"إهلاك الأصول - العملية {run.RunNumber} - أصل {schedule.Asset.Code}";

            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.Id,
                Sequence = sequence++,
                AccountId = debitAccountId.Value,
                Debit = amount,
                Credit = 0,
                Description = description,
                CurrencyId = currencyId,
                ExchangeRate = 1,
                DepreciationScheduleLineId = schedule.Id
            });
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.Id,
                Sequence = sequence++,
                AccountId = creditAccountId.Value,
                Debit = 0,
                Credit = amount,
                Description = description,
                CurrencyId = currencyId,
                ExchangeRate = 1,
                DepreciationScheduleLineId = schedule.Id
            });
        }

        run.JournalEntryId = entry.Id;
        await context.SaveChangesAsync(ct);
    }

    private async Task<JournalEntry?> FindExistingEntryAsync(int runId, int? linkedEntryId, CancellationToken ct)
    {
        if (linkedEntryId.HasValue)
        {
            var linked = await context.JournalEntries.FindAsync([linkedEntryId.Value], ct);
            if (linked is not null) return linked;
        }

        var entryId = await context.JournalEntryLines
            .Where(l => l.DepreciationScheduleLineId != null &&
                        l.DepreciationScheduleLine!.DepreciationRunId == runId)
            .Select(l => l.JournalEntryId)
            .FirstOrDefaultAsync(ct);
        return entryId == 0 ? null : await context.JournalEntries.FindAsync([entryId], ct);
    }

    private static decimal CalculateResidualValue(ERP_Government.Domain.Assets.Entities.Asset asset)
    {
        var residualPercentage = asset.AssetGroup?.ResidualValuePercentage ?? 0;
        return Math.Round(asset.OriginalValue * residualPercentage / 100, 2);
    }
}
