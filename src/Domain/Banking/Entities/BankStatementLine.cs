using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Banking.Entities;

public class BankStatementLine : BaseAuditableEntity
{
    public int StatementId { get; set; }
    public int LineNumber { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal? Balance { get; set; }
    public string? Reference { get; set; }
    public bool IsReconciled { get; set; }
    public long? JournalEntryLineId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public BankStatement Statement { get; set; } = null!;
}
