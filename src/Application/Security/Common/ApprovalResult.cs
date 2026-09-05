namespace ERP_Government.Application.Security.Common;

public class ApprovalResult
{
    public bool Success { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public int? ApprovalHistoryId { get; init; }
}
