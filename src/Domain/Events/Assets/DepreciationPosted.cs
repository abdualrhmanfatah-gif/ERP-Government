using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Assets;

public class DepreciationPosted : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "DepreciationSchedule";
    public DateTimeOffset OccurredAt { get; init; }
    public int AssetId { get; init; }
    public decimal DepreciationAmount { get; init; }
    public int PeriodId { get; init; }
}
