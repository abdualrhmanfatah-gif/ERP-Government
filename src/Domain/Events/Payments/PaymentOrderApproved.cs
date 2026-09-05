using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Payments;

public class PaymentOrderApproved : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "PaymentOrder";
    public DateTimeOffset OccurredAt { get; init; }
    public int VendorId { get; init; }
    public decimal TotalAmount { get; init; }
    public int CurrencyId { get; init; }
}
