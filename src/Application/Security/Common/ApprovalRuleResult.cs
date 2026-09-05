namespace ERP_Government.Application.Security.Common;

public class ApprovalRuleResult
{
    public int Sequence { get; init; }
    public string RequiredRole { get; init; } = string.Empty;
    public int? ApproverRoleId { get; init; }
}
