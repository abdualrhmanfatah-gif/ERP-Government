using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;

namespace ERP_Government.Application.Accounting.Integration.Assets;

public class AssetImpairmentReversedHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService)
    : INotificationHandler<AssetImpairmentReversed>
{
    public async Task Handle(AssetImpairmentReversed notification, CancellationToken cancellationToken)
    {
        var reversalTransaction = await context.AssetTransactions.FindAsync(
            [notification.SourceEntityId], cancellationToken);
        if (reversalTransaction is null) return;
        if (reversalTransaction.JournalEntryId is not null || reversalTransaction.IsPosted) return;

        var originalTransaction = await context.AssetTransactions.FindAsync(
            [notification.ReversalOfTransactionId], cancellationToken);
        if (originalTransaction?.JournalEntryId is null) return;

        var originalEntry = await context.JournalEntries.FindAsync(
            [originalTransaction.JournalEntryId.Value], cancellationToken);
        if (originalEntry is null) return;

        var originalLines = await context.JournalEntryLines
            .Where(l => l.JournalEntryId == originalEntry.Id)
            .OrderBy(l => l.Sequence)
            .ToListAsync(cancellationToken);

        var asset = await context.Assets.FindAsync([notification.AssetId], cancellationToken);
        if (asset is null) return;

        var period = await context.FiscalPeriods.FindAsync([notification.PeriodId], cancellationToken);
        if (period is null) return;

        var entryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = reversalTransaction.TransactionDate,
            PostingDate = DateOnly.FromDateTime(DateTime.Today),
            EntryStatus = EntryStatus.Posted,
            EntryType = MoveEntryType.SystemGenerated,
            PeriodId = period.Id,
            FiscalYearId = period.FiscalYearId,
            IsSystemGenerated = true,
            Narration = $"عكس انخفاض قيمة — أصل #{asset.Code} — المعاملة الأصلية #{notification.ReversalOfTransactionId}",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);

        var seq = 1;
        foreach (var line in originalLines)
        {
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.Id,
                Sequence = seq++,
                AccountId = line.AccountId,
                Debit = line.Credit,
                Credit = line.Debit,
                Description = $"عكس — {line.Description}",
                CurrencyId = line.CurrencyId,
                ExchangeRate = line.ExchangeRate
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        asset.CurrentValue = (asset.CurrentValue ?? asset.OriginalValue) + notification.ReversalAmount;
        asset.LastModified = DateTimeOffset.UtcNow;

        reversalTransaction.JournalEntryId = entry.Id;
        reversalTransaction.Status = AssetTransactionStatus.Posted;
        reversalTransaction.IsPosted = true;
        reversalTransaction.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
