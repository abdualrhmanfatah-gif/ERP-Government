using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Revenue;

public class CheckClearedEvent : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "Check";
    public DateTimeOffset OccurredAt { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string CheckNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public List<CheckClearedRevenueLineDetail> RevenueLines { get; init; } = [];
}

public record CheckClearedRevenueLineDetail(int RevenueAccountId, decimal Amount, string? Description);
