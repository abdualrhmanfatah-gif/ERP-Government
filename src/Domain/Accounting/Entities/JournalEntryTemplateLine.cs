using ERP_Government.Domain.Common;
using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Journal entry template lines — strict mirror of JournalEntryLine (except FK).
/// </summary>
public class JournalEntryTemplateLine : BaseAuditableEntity
{
    public int TemplateId { get; set; }
    public int Sequence { get; set; }
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int? CostCenterId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public JournalEntryTemplate Template { get; set; } = null!;
    public Account Account { get; set; } = null!;

    // Cross-module FKs (Module 3)
    public CostCenter? CostCenter { get; set; }
}
