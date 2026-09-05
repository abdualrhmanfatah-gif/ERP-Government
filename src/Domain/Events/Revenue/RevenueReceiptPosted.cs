using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Revenue;

public class RevenueReceiptPosted : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "RevenueReceipt";
    public DateTimeOffset OccurredAt { get; init; }
    public string PayerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int CurrencyId { get; init; }
}
