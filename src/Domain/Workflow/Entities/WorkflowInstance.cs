using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Workflow.Entities;

public class WorkflowInstance : BaseAuditableEntity
{
    public int DefinitionId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? CurrentStepId { get; set; }
    public string Status { get; set; } = string.Empty; // WorkflowInstanceStatus enum as string
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    // Navigation properties
    public WorkflowDefinition? Definition { get; set; }
    public WorkflowStep? CurrentStep { get; set; }
    public ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
}