using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Workflow.Entities;

public class WorkflowStep : BaseEntity
{
    public int DefinitionId { get; set; }
    public int StepOrder { get; set; }
    public string StepType { get; set; } = string.Empty; // WorkflowStepType enum as string
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AssignedRoleId { get; set; }
    public bool UseApprovalRules { get; set; }
    public string? ConditionExpression { get; set; } // JSON condition
    public int? TimeoutHours { get; set; }
    public int? EscalateToStepId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public WorkflowDefinition? Definition { get; set; }
    public WorkflowStep? EscalateToStep { get; set; }
    public ICollection<WorkflowStep> EscalationTargets { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowInstance> CurrentInstances { get; set; } = new List<WorkflowInstance>();
    public ICollection<WorkflowHistory> HistoryRecords { get; set; } = new List<WorkflowHistory>();
}