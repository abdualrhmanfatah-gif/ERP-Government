using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Domain.Events.Revenue;

public class ReceiptVoucherCollected : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "ReceiptVoucher";
    public DateTimeOffset OccurredAt { get; init; }
    public string PartyName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int CurrencyId { get; init; }
    public FormType FormType { get; init; }
}
