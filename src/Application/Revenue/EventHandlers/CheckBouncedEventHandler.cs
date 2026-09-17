using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.EventHandlers;

public class CheckBouncedEventHandler(RevenueJournalEntryService journalService)
    : INotificationHandler<CheckBouncedEvent>
{
    public async Task Handle(CheckBouncedEvent notification, CancellationToken cancellationToken)
    {
        var pendingCheckReceiptsAccId = await journalService.GetOrCreateSystemAccountAsync(
            "210901",
            "متحصلات شيكات معلقة",
            NormalBalanceType.Credit,
            cancellationToken);

        var checksUnderCollectionAccId = await journalService.GetOrCreateSystemAccountAsync(
            "110202",
            "شيكات تحت التحصيل",
            NormalBalanceType.Debit,
            cancellationToken);

        var lines = new List<(int AccountId, decimal Debit, decimal Credit, string? Description)>
        {
            (pendingCheckReceiptsAccId, notification.Amount, 0, $"إلغاء التزام الشيك المعلق بسبب الارتجاع — شيك #{notification.CheckNumber}"),
            (checksUnderCollectionAccId, 0, notification.Amount, $"إلغاء شيك تحت التحصيل مرتجع — شيك #{notification.CheckNumber}")
        };

        await journalService.CreateSystemJournalEntryAsync(
            $"إلغاء وتصفية القيد الوسيط للشيك المرتجع رقم #{notification.CheckNumber} (السبب: {notification.Reason ?? "غير محدد"})",
            lines,
            cancellationToken);
    }
}
