using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Assets;

public class AssetDisposed : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "AssetDisposal";
    public DateTimeOffset OccurredAt { get; init; }
    public int AssetId { get; init; }
    public decimal NetBookValue { get; init; }
    public decimal? SalePrice { get; init; }
}
