using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetRevaluation : BaseAuditableEntity
{
    public string RevaluationNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public DateOnly RevaluationDate { get; set; }
    public string? RevaluationMethod { get; set; }
    public decimal OldBookValue { get; set; }
    public decimal NewBookValue { get; set; }
    public decimal RevaluationAmount { get; set; }
    public string? RevaluationType { get; set; }
    public string? Appraiser { get; set; }
    public string? AppraisalReportNumber { get; set; }
    public string? CurrencyCode { get; set; }
    public int? AccountRevaluationId { get; set; }
    public int? JournalEntryId { get; set; }
    public bool IsPosted { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Asset? Asset { get; set; }
}
