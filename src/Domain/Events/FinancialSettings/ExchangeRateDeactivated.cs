using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.FinancialSettings;

public class ExchangeRateDeactivated : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "ExchangeRate";
    public DateTimeOffset OccurredAt { get; init; }
    public int ExchangeRateId { get; init; }
    public string CurrencyPair { get; init; } = string.Empty;
}
