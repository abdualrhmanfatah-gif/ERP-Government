using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Payments;

public class PaymentOrderExecuted : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "PaymentExecution";
    public DateTimeOffset OccurredAt { get; init; }
    public int PaymentOrderId { get; init; }
    public decimal Amount { get; init; }
    public int BankAccountId { get; init; }
}
