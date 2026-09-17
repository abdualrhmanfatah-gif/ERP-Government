using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.EventHandlers;

public class CashReceiptApprovedEventHandler(RevenueJournalEntryService journalService)
    : INotificationHandler<CashReceiptApprovedEvent>
{
    public async Task Handle(CashReceiptApprovedEvent notification, CancellationToken cancellationToken)
    {
        var cashierAccountId = await journalService.GetOrCreateSystemAccountAsync(
            "1812",
            "نقدية لدى أمين الصندوق",
            NormalBalanceType.Debit,
            cancellationToken);

        var lines = new List<(int AccountId, decimal Debit, decimal Credit, string? Description)>
        {
            (cashierAccountId, notification.TotalAmount, 0, $"تحصيل نقدي — سند قبض #{notification.VoucherNumber}")
        };

        foreach (var l in notification.Lines)
        {
            lines.Add((l.RevenueAccountId, 0, l.Amount, l.Description ?? $"إيراد — سند قبض #{notification.VoucherNumber}"));
        }

        await journalService.CreateSystemJournalEntryAsync(
            $"إثبات الإيراد النقدي بموجب سند القبض #{notification.VoucherNumber} — {notification.ReceivedFrom}",
            lines,
            cancellationToken);
    }
}
