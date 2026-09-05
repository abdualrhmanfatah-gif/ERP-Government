using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.FinancialSettings;

public class ExchangeRateActivated : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "ExchangeRate";
    public DateTimeOffset OccurredAt { get; init; }
    public int ExchangeRateId { get; init; }
    public string CurrencyPair { get; init; } = string.Empty;
    public decimal Rate { get; init; }
}
