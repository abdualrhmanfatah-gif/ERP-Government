using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Assets;

public class AssetImpairmentReversed : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "AssetImpairment";
    public DateTimeOffset OccurredAt { get; init; }
    public int AssetId { get; init; }
    public decimal ReversalAmount { get; init; }
    public int ReversalOfTransactionId { get; init; }
    public int PeriodId { get; init; }
}
