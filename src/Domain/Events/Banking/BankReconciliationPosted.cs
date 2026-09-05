using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Banking;

public class BankReconciliationPosted : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "BankReconciliation";
    public DateTimeOffset OccurredAt { get; init; }
    public int BankAccountId { get; init; }
    public decimal ReconciledAmount { get; init; }
}
