using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Accounting;

public class JournalEntryReversed : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "JournalEntry";
    public DateTimeOffset OccurredAt { get; init; }
    public long JournalEntryId { get; init; }
    public long ReversalOfId { get; init; }
    public string? ReversalReason { get; init; }
}
