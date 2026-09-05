namespace ERP_Government.Application.Security.Common;

public class ApprovalRuleDto
{
    public int Id { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public int? FundId { get; init; }
    public decimal? AmountThreshold { get; init; }
    public int? CurrencyId { get; init; }
    public string? ApproverRole { get; init; }
    public int? ApproverRoleId { get; init; }
    public int Sequence { get; init; }
    public bool IsActive { get; init; }
}
