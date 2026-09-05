using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Procurement;

public class PurchaseOrderApproved : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "PurchaseOrder";
    public DateTimeOffset OccurredAt { get; init; }
    public int SupplierId { get; init; }
    public decimal? GrandTotal { get; init; }
    public string? CurrencyCode { get; init; }
}
