namespace ERP_Government.Application.Security.Common;

public class ApprovalHistoryDto
{
    public int Id { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public int DocumentId { get; init; }
    public int ApproverUserId { get; init; }
    public string RequiredRole { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public DateTimeOffset DecisionAt { get; init; }
    public string? Reason { get; init; }
    public string? EvaluationSnapshot { get; init; }
}
