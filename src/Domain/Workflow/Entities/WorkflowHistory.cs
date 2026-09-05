using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Workflow.Entities;

public class WorkflowHistory : BaseEntity
{
    public int WorkflowInstanceId { get; set; }
    public int StepId { get; set; }
    public int ActorUserId { get; set; }
    public string Decision { get; set; } = string.Empty; // WorkflowDecision enum as string
    public string? Reason { get; set; }
    public string? EvaluationSnapshot { get; set; } // JSON
    public DateTimeOffset Timestamp { get; set; }

    // Navigation properties
    public WorkflowInstance? WorkflowInstance { get; set; }
    public WorkflowStep? Step { get; set; }
}