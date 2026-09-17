using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Revenue;

public class DepositSlip48ApprovedEvent : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "DepositSlip48";
    public DateTimeOffset OccurredAt { get; init; }
    public string SlipNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
}
