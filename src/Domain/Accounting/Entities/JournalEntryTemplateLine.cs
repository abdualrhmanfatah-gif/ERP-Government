using ERP_Government.Domain.Common;
using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Journal entry template lines per schema table 44.
/// </summary>
public class JournalEntryTemplateLine : BaseAuditableEntity
{
    public int TemplateId { get; set; }
    public int Sequence { get; set; }
    public int AccountId { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
    public int? FundId { get; set; } // Deferred FK to Module 5 — Funds table not yet implemented
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public int? OrganizationUnitId { get; set; }
    public int? CurrencyId { get; set; }
    public bool IsMandatory { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public JournalEntryTemplate Template { get; set; } = null!;
    public Account Account { get; set; } = null!;

    // Cross-module FKs (Module 3)
    public CostCenter? CostCenter { get; set; }
    public OrganizationalUnit? OrganizationUnit { get; set; }
    public Project? Project { get; set; }

    // Cross-module FKs (Module 1)
    // public Currency? Currency { get; set; }
}
