using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Assets;

public class AssetAcquired : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "Asset";
    public DateTimeOffset OccurredAt { get; init; }
    public string AssetCode { get; init; } = string.Empty;
    public decimal OriginalValue { get; init; }
    public int CurrencyId { get; init; }
}
