using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Banking.Entities;

public class BankReconciliationLine : BaseAuditableEntity
{
    public int ReconciliationId { get; set; }
    public Enums.ReconciliationLineType LineType { get; set; }
    public int? BankStatementLineId { get; set; }
    public long? JournalEntryLineId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public Enums.ReconciliationLineStatus Status { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public BankReconciliation Reconciliation { get; set; } = null!;
    public BankStatementLine? BankStatementLine { get; set; }
}
