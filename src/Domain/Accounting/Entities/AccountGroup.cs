using System.ComponentModel.DataAnnotations.Schema;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Account groups (Assets, Liabilities, Equity, Revenue, Expenses) per schema table 35.
/// </summary>
public class AccountGroup : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountGroupType Type { get; set; }
    public NormalBalanceType NormalBalance { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public byte Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public AccountGroup? Parent { get; set; }

    /// <summary>Transient — used only during seeding to resolve ParentId from Code.</summary>
    [NotMapped]
    public string? ParentCode { get; set; }
}
