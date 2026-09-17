using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.EventHandlers;

public class CheckClearedEventHandler(RevenueJournalEntryService journalService)
    : INotificationHandler<CheckClearedEvent>
{
    public async Task Handle(CheckClearedEvent notification, CancellationToken cancellationToken)
    {
        var bankAccountId = await journalService.GetOrCreateSystemAccountAsync(
            "110102",
            "حساب البنك / الخزينة المركزية",
            NormalBalanceType.Debit,
            cancellationToken);

        var checksUnderCollectionAccId = await journalService.GetOrCreateSystemAccountAsync(
            "110202",
            "شيكات تحت التحصيل",
            NormalBalanceType.Debit,
            cancellationToken);

        var pendingCheckReceiptsAccId = await journalService.GetOrCreateSystemAccountAsync(
            "210901",
            "متحصلات شيكات معلقة",
            NormalBalanceType.Credit,
            cancellationToken);

        // Entry 1: Transfer check amount from UnderCollection to Bank
        var bankLines = new List<(int AccountId, decimal Debit, decimal Credit, string? Description)>
        {
            (bankAccountId, notification.Amount, 0, $"تصفية شيك #{notification.CheckNumber} — بنك {notification.BankName}"),
            (checksUnderCollectionAccId, 0, notification.Amount, $"سداد شيك تحت التحصيل #{notification.CheckNumber}")
        };

        await journalService.CreateSystemJournalEntryAsync(
            $"إثبات دخول قيمة الشيك المصفى رقم #{notification.CheckNumber} إلى البنك",
            bankLines,
            cancellationToken);

        // Entry 2: Recognize Revenue from Pending Receipts
        var revenueLines = new List<(int AccountId, decimal Debit, decimal Credit, string? Description)>
        {
            (pendingCheckReceiptsAccId, notification.Amount, 0, $"إقفال الالتزام المعلق لتصفية شيك #{notification.CheckNumber}")
        };

        if (notification.RevenueLines.Count > 0)
        {
            foreach (var rl in notification.RevenueLines)
            {
                revenueLines.Add((rl.RevenueAccountId, 0, rl.Amount, rl.Description ?? $"الاعتراف بالإيراد عند تصفية شيك #{notification.CheckNumber}"));
            }
        }

        await journalService.CreateSystemJournalEntryAsync(
            $"الاعتراف الفعلي بالإيراد عند تصفية الشيك رقم #{notification.CheckNumber}",
            revenueLines,
            cancellationToken);
    }
}
