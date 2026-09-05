using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class RecordRule : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public RuleType RuleType { get; set; }
    public AccessScope AccessScope { get; set; }
    public string? DomainFilter { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public SecurityRole? Role { get; set; }
}
