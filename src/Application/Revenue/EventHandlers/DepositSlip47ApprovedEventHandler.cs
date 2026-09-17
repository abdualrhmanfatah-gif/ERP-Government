using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.EventHandlers;

public class DepositSlip47ApprovedEventHandler(RevenueJournalEntryService journalService)
    : INotificationHandler<DepositSlip47ApprovedEvent>
{
    public async Task Handle(DepositSlip47ApprovedEvent notification, CancellationToken cancellationToken)
    {
        var bankAccountId = await journalService.GetOrCreateSystemAccountAsync(
            "110102",
            "حساب البنك / الخزينة المركزية",
            NormalBalanceType.Debit,
            cancellationToken);

        var cashierAccountId = await journalService.GetOrCreateSystemAccountAsync(
            "1812",
            "نقدية لدى أمين الصندوق",
            NormalBalanceType.Debit,
            cancellationToken);

        var lines = new List<(int AccountId, decimal Debit, decimal Credit, string? Description)>
        {
            (bankAccountId, notification.TotalAmount, 0, $"توريد نقد للبنك — حافظة 47 #{notification.SlipNumber}"),
            (cashierAccountId, 0, notification.TotalAmount, $"إخلاء عهدة الصندوق — حافظة 47 #{notification.SlipNumber}")
        };

        await journalService.CreateSystemJournalEntryAsync(
            $"نقل النقدية من عهدة أمين الصندوق إلى البنك بموجب حافظة التوريد 47 #{notification.SlipNumber}",
            lines,
            cancellationToken);
    }
}
