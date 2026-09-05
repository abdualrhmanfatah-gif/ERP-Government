namespace ERP_Government.Application.Security.Common;

public class PendingApprovalDto
{
    public string DocumentType { get; init; } = string.Empty;
    public int DocumentId { get; init; }
    public decimal Amount { get; init; }
    public int SubmittedByUserId { get; init; }
    public DateTimeOffset SubmittedAt { get; init; }
    public string RequiredRole { get; init; } = string.Empty;
}
