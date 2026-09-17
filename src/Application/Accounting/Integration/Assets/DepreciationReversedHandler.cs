using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Assets.Services;
using ERP_Government.Domain.Events.Assets;

namespace ERP_Government.Application.Accounting.Integration.Assets;

public class DepreciationReversedHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : INotificationHandler<DepreciationReversed>
{
    public async Task Handle(DepreciationReversed notification, CancellationToken ct)
    {
        var run = await context.AssetDepreciationRuns
            .Include(r => r.Schedules)
                .ThenInclude(s => s.Asset)
            .FirstOrDefaultAsync(r => r.Id == notification.SourceEntityId, ct);
        if (run?.JournalEntryId is null || run.Status == AssetDepreciationRunStatus.Reversed) return;

        var originalEntry = await context.JournalEntries.FindAsync([run.JournalEntryId.Value], ct);
        if (originalEntry is null) return;

        var reversalEntry = run.ReversalJournalEntryId.HasValue
            ? await context.JournalEntries.FindAsync([run.ReversalJournalEntryId.Value], ct)
            : null;

        if (reversalEntry is null)
        {
            var originalLines = await context.JournalEntryLines
                .Where(l => l.JournalEntryId == originalEntry.Id)
                .OrderBy(l => l.Sequence)
                .ToListAsync(ct);

            reversalEntry = new JournalEntry
            {
                EntryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", ct),
                Ref = run.RunNumber,
                DocumentDate = notification.ReversalDate,
                PostingDate = DateOnly.FromDateTime(DateTime.Today),
                EntryStatus = EntryStatus.Posted,
                EntryType = MoveEntryType.Accrual,
                PeriodId = notification.PeriodId,
                FiscalYearId = originalEntry.FiscalYearId,
                ReversalOfId = originalEntry.Id,
                ReversalReason = run.ReversalReason,
                IsSystemGenerated = true,
                Narration = $"عكس إهلاك الأصول - العملية {run.RunNumber}",
                PostedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System"
            };
            context.JournalEntries.Add(reversalEntry);
            await context.SaveChangesAsync(ct);

            foreach (var line in originalLines)
            {
                context.JournalEntryLines.Add(new JournalEntryLine
                {
                    JournalEntryId = reversalEntry.Id,
                    Sequence = line.Sequence,
                    AccountId = line.AccountId,
                    Debit = line.Credit,
                    Credit = line.Debit,
                    Description = $"عكس - {line.Description}",
                    CurrencyId = line.CurrencyId,
                    ExchangeRate = line.ExchangeRate,
                    AssetDepreciationRunId = run.Id
                });
            }
        }

        foreach (var schedule in run.Schedules)
        {
            var asset = schedule.Asset!;
            asset.AccumulatedDepreciation = schedule.OpeningAccumulatedDepreciation;
            asset.CurrentValue = schedule.OpeningBookValue;
            asset.IsFullyDepreciated = DepreciationCalculator.IsFullyDepreciated(asset, asset.AccumulatedDepreciation);

            asset.LastDepreciationDate = await context.DepreciationSchedules
                .Where(s => s.AssetId == asset.Id
                    && s.DepreciationRunId != run.Id
                    && s.DepreciationRun!.Status == AssetDepreciationRunStatus.Posted)
                .MaxAsync(s => (DateOnly?)s.DepreciationRun!.DepreciationDate, ct);
        }

        run.ReversalJournalEntryId = reversalEntry.Id;
        run.Status = AssetDepreciationRunStatus.Reversed;
        run.ReversedAt = DateTimeOffset.UtcNow;
        run.ReversedBy = "System";
        await context.SaveChangesAsync(ct);
    }
}
