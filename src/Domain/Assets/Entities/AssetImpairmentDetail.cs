using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetImpairmentDetail : BaseEntity
{
    public int AssetTransactionId { get; set; }
    public decimal CarryingAmount { get; set; }
    public decimal RecoverableAmount { get; set; }
    public decimal LossAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssessorName { get; set; }
    public string? ReportNumber { get; set; }
    public int? ReversalOfTransactionId { get; set; }
    public string? ReversalReason { get; set; }

    public AssetTransaction? AssetTransaction { get; set; }
    public AssetTransaction? ReversalOfTransaction { get; set; }
}
