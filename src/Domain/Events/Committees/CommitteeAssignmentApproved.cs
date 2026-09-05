using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.Committees;

public class CommitteeAssignmentApproved : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "CommitteeAssignment";
    public DateTimeOffset OccurredAt { get; init; }
    public int CommitteeId { get; init; }
    public string AssignmentType { get; init; } = string.Empty;
}
