namespace ERP_Government.Shared.Workflow;

public class WorkflowStepDto
{
    public int Id { get; set; }
    public int DefinitionId { get; set; }
    public int StepOrder { get; set; }
    public string StepType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AssignedRoleId { get; set; }
    public bool UseApprovalRules { get; set; }
    public string? ConditionExpression { get; set; }
    public int? TimeoutHours { get; set; }
    public int? EscalateToStepId { get; set; }
    public bool IsActive { get; set; }
}