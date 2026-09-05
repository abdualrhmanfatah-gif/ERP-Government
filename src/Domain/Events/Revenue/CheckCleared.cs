using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Revenue;

public class CheckCleared : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "Check";
    public DateTimeOffset OccurredAt { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string CheckNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int CurrencyId { get; init; }
}
