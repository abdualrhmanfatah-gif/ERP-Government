using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Revenue;

public class CashReceiptApprovedEvent : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "ReceiptVoucher";
    public DateTimeOffset OccurredAt { get; init; }
    public string VoucherNumber { get; init; } = string.Empty;
    public string ReceivedFrom { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public List<CashReceiptLineDetail> Lines { get; init; } = [];
}

public record CashReceiptLineDetail(int RevenueAccountId, decimal Amount, string? Description);
