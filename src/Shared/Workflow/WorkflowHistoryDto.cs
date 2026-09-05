namespace ERP_Government.Shared.Workflow;

public class WorkflowHistoryDto
{
    public int Id { get; set; }
    public int WorkflowInstanceId { get; set; }
    public int StepId { get; set; }
    public int ActorUserId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? EvaluationSnapshot { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}