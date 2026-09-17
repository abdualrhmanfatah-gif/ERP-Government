using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.EventHandlers;

public class CheckReceiptApprovedEventHandler(RevenueJournalEntryService journalService)
    : INotificationHandler<CheckReceiptApprovedEvent>
{
    public async Task Handle(CheckReceiptApprovedEvent notification, CancellationToken cancellationToken)
    {
        var checksPendingDepositAccId = await journalService.GetOrCreateSystemAccountAsync(
            "110201",
            "شيكات برسم الإيداع",
            NormalBalanceType.Debit,
            cancellationToken);

        var pendingCheckReceiptsAccId = await journalService.GetOrCreateSystemAccountAsync(
            "210901",
            "متحصلات شيكات معلقة",
            NormalBalanceType.Credit,
            cancellationToken);

        var lines = new List<(int AccountId, decimal Debit, decimal Credit, string? Description)>
        {
            (checksPendingDepositAccId, notification.TotalAmount, 0, $"استلام شيكات — سند قبض #{notification.VoucherNumber}"),
            (pendingCheckReceiptsAccId, 0, notification.TotalAmount, $"التزام معلق لشيكات سند قبض #{notification.VoucherNumber}")
        };

        await journalService.CreateSystemJournalEntryAsync(
            $"القيد الوسيط لإثبات الشيكات المستلمة بموجب سند القبض #{notification.VoucherNumber} — {notification.ReceivedFrom}",
            lines,
            cancellationToken);
    }
}
