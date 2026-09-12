using ERP_Government.Domain.Common;
using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Domain.Accounting.Entities;

public class JournalEntryLine : BaseLongAuditableEntity
{
    public int JournalEntryId { get; set; }
    public int Sequence { get; set; }
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int? CostCenterId { get; set; }
    public int? PaymentOrderId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    // Same-module FKs
    public JournalEntry JournalEntry { get; set; } = null!;
    public Account Account { get; set; } = null!;

    // Cross-module FKs
    public CostCenter? CostCenter { get; set; }
}
