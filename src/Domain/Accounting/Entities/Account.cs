using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Chart of Accounts per schema table 36.
/// </summary>
public class Account : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AccountGroupId { get; set; }
    public int? ParentId { get; set; }
    public byte Level { get; set; }
    public NormalBalanceType NormalBalance { get; set; }
    public bool IsPostable { get; set; } = true;
    public bool IsReconcilable { get; set; }
    public int? CurrencyId { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public AccountGroup AccountGroup { get; set; } = null!;
    public Account? Parent { get; set; }
    // Deferred FK to Module 1 — Currencies table already implemented
    // public Currency? Currency { get; set; }
}
