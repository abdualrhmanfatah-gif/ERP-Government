using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Assets;

public class DepreciationReversed : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "AssetDepreciationRun";
    public DateTimeOffset OccurredAt { get; init; }
    public int PeriodId { get; init; }
    public DateOnly ReversalDate { get; init; }
}
