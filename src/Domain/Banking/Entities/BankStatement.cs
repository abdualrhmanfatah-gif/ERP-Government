using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Banking.Entities;

public class BankStatement : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int BankAccountId { get; set; }
    public int JournalId { get; set; }
    public DateOnly StatementDate { get; set; }
    public decimal BalanceStart { get; set; }
    public decimal BalanceEnd { get; set; }
    public decimal BalanceEndComputed { get; set; }
    public string? ImportSource { get; set; }
    public Enums.BankStatementStatus Status { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
