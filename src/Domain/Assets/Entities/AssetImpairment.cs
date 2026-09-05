using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetImpairment : BaseAuditableEntity
{
    public string ImpairmentNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public DateOnly ImpairmentDate { get; set; }
    public decimal CarryingAmount { get; set; }
    public decimal RecoverableAmount { get; set; }
    public decimal ImpairmentLoss { get; set; }
    public string? ImpairmentReason { get; set; }
    public string? ImpairmentDescription { get; set; }
    public string? AssessedBy { get; set; }
    public string? AssessmentReportNumber { get; set; }
    public string? CurrencyCode { get; set; }
    public int? AccountImpairmentId { get; set; }
    public int? JournalEntryId { get; set; }
    public bool IsPosted { get; set; }
    public bool IsReversed { get; set; }
    public int? ReversalOfId { get; set; }
    public string? ReversalReason { get; set; }
    public DateOnly? ReversalDate { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Asset? Asset { get; set; }
    public AssetImpairment? ReversalOf { get; set; }
}
