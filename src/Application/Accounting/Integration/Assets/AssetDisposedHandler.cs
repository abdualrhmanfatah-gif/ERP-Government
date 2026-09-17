using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;

namespace ERP_Government.Application.Accounting.Integration.Assets;

public class AssetDisposedHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService)
    : INotificationHandler<AssetDisposed>
{
    public async Task Handle(AssetDisposed notification, CancellationToken cancellationToken)
    {
        var transaction = await context.AssetTransactions.FindAsync(
            [notification.SourceEntityId], cancellationToken);
        if (transaction is null) return;
        if (transaction.JournalEntryId is not null || transaction.IsPosted) return;

        var asset = await context.Assets.FindAsync([notification.AssetId], cancellationToken);
        if (asset is null) return;

        var group = await context.AssetGroups.FindAsync([asset.AssetGroupId], cancellationToken);
        if (group is null) return;

        var disposalAccountId = group.DisposalAccountId;
        var assetAccountId = group.AssetAccountId;
        if (disposalAccountId is null || assetAccountId is null) return;

        var period = await context.FiscalPeriods.FindAsync([notification.PeriodId], cancellationToken);
        if (period is null) return;

        var entryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = transaction.TransactionDate,
            PostingDate = DateOnly.FromDateTime(DateTime.Today),
            EntryStatus = EntryStatus.Posted,
            EntryType = MoveEntryType.SystemGenerated,
            PeriodId = period.Id,
            FiscalYearId = period.FiscalYearId,
            IsSystemGenerated = true,
            Narration = $"تخليص أصل #{asset.Code}",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);

        var seq = 1;

        if (asset.AccumulatedDepreciation > 0)
        {
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.Id,
                Sequence = seq++,
                AccountId = group.AccumulatedDepreciationAccountId ?? disposalAccountId.Value,
                Debit = asset.AccumulatedDepreciation,
                Credit = 0,
                Description = $"إهلاك متراكم — تخليص #{asset.Code}",
                CurrencyId = asset.CurrencyId,
                ExchangeRate = 1
            });
        }

        context.JournalEntryLines.Add(new JournalEntryLine
        {
            JournalEntryId = entry.Id,
            Sequence = seq++,
            AccountId = disposalAccountId.Value,
            Debit = notification.NetBookValue,
            Credit = 0,
            Description = $"قيمة دفترية — تخليص #{asset.Code}",
            CurrencyId = asset.CurrencyId,
            ExchangeRate = 1
        });

        context.JournalEntryLines.Add(new JournalEntryLine
        {
            JournalEntryId = entry.Id,
            Sequence = seq++,
            AccountId = assetAccountId.Value,
            Debit = 0,
            Credit = asset.OriginalValue,
            Description = $"قيمة أصل — تخليص #{asset.Code}",
            CurrencyId = asset.CurrencyId,
            ExchangeRate = 1
        });

        if (notification.SalePrice is > 0)
        {
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.Id,
                Sequence = seq++,
                AccountId = disposalAccountId.Value,
                Debit = 0,
                Credit = notification.SalePrice.Value,
                Description = $"مبيعات — تخليص #{asset.Code}",
                CurrencyId = asset.CurrencyId,
                ExchangeRate = 1
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        asset.Status = "Disposed";
        asset.LastModified = DateTimeOffset.UtcNow;

        transaction.JournalEntryId = entry.Id;
        transaction.Status = AssetTransactionStatus.Posted;
        transaction.IsPosted = true;
        transaction.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
