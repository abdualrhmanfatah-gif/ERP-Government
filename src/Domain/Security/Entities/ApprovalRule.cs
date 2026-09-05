using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

public class ApprovalRule : BaseAuditableEntity
{
    public string DocumentType { get; set; } = string.Empty;
    public int? FundId { get; set; }
    public decimal? AmountThreshold { get; set; }
    public int? CurrencyId { get; set; }
    public int? ApproverRoleId { get; set; }
    public string? ApproverRole { get; set; }
    public int Sequence { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public SecurityRole? ApproverRoleNavigation { get; set; }
}
