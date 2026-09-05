namespace ERP_Government.Shared.Workflow;

public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public int DefinitionId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? CurrentStepId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public List<WorkflowHistoryDto> History { get; set; } = new();
}