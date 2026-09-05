namespace ERP_Government.Shared.Workflow;

public class WorkflowDefinitionDto
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}