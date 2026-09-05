using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Budgeting;

public class ClosingEntryGenerated : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "FinalAccount";
    public DateTimeOffset OccurredAt { get; init; }
    public int FinalAccountId { get; init; }
    public int FiscalYearId { get; init; }
}
