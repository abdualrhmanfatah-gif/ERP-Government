using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Banking.Entities;

public class BankReconciliation : BaseAuditableEntity
{
    public int BankAccountId { get; set; }
    public int StatementId { get; set; }
    public DateOnly ReconciliationDate { get; set; }
    public decimal BookBalance { get; set; }
    public decimal StatementBalance { get; set; }
    public decimal AdjustedBalance { get; set; }
    public decimal? Difference { get; set; }
    public Enums.ReconciliationStatus Status { get; set; }
    public int PreparedById { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public BankStatement Statement { get; set; } = null!;
}
