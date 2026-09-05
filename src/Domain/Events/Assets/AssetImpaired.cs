using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Assets;

public class AssetImpaired : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "AssetImpairment";
    public DateTimeOffset OccurredAt { get; init; }
    public int AssetId { get; init; }
    public decimal PreviousValue { get; init; }
    public decimal RecoverableAmount { get; init; }
}
