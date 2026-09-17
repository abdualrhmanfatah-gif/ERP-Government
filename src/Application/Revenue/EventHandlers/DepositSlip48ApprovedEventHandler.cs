using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.EventHandlers;

public class DepositSlip48ApprovedEventHandler(RevenueJournalEntryService journalService)
    : INotificationHandler<DepositSlip48ApprovedEvent>
{
    public async Task Handle(DepositSlip48ApprovedEvent notification, CancellationToken cancellationToken)
    {
        var checksUnderCollectionAccId = await journalService.GetOrCreateSystemAccountAsync(
            "110202",
            "شيكات تحت التحصيل",
            NormalBalanceType.Debit,
            cancellationToken);

        var checksPendingDepositAccId = await journalService.GetOrCreateSystemAccountAsync(
            "110201",
            "شيكات برسم الإيداع",
            NormalBalanceType.Debit,
            cancellationToken);

        var lines = new List<(int AccountId, decimal Debit, decimal Credit, string? Description)>
        {
            (checksUnderCollectionAccId, notification.TotalAmount, 0, $"إرسال شيكات للتحصيل — حافظة 48 #{notification.SlipNumber}"),
            (checksPendingDepositAccId, 0, notification.TotalAmount, $"تحويل من شيكات برسم الإيداع — حافظة 48 #{notification.SlipNumber}")
        };

        await journalService.CreateSystemJournalEntryAsync(
            $"إثبات إرسال الشيكات للبنك للمقاصة والتحصيل بموجب حافظة 48 #{notification.SlipNumber}",
            lines,
            cancellationToken);
    }
}
