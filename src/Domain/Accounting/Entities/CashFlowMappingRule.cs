using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Accounting.Entities;

public class CashFlowMappingRule : BaseAuditableEntity
{
    public int AccountGroupId { get; set; }

    public CashFlowSectionType Section { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public byte[]? RowVersion { get; set; }

    public AccountGroup AccountGroup { get; set; } = default!;
}
