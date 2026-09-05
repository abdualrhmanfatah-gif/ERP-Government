using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Revenue;

public class RevenueReceiptApproved : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "RevenueReceipt";
    public DateTimeOffset OccurredAt { get; init; }
}
