using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Workflow.Entities;

public class WorkflowDefinition : BaseAuditableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty; // WorkflowDefinitionStatus enum as string
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    // Navigation properties
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}