using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class ApprovalHistory : BaseAuditableEntity
{
    public string DocumentType { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public int ApprovalStep { get; set; } = 1;
    public ApprovalAction Action { get; set; }
    public int ApproverUserId { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public DateTimeOffset DecisionAt { get; set; }
    public string? Reason { get; set; }
    public string? EvaluationSnapshot { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public User ApproverUser { get; set; } = null!;
}
