using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class SecurityRole : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoleLevel RoleLevel { get; set; }
    public bool IsMutuallyExclusive { get; set; }
    public int? ExclusiveWithRoleId { get; set; }
    public bool RequiresMfa { get; set; }
    public int? MaxSessionDuration { get; set; }
    public bool IsSystem { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public SecurityRole? ExclusiveWithRole { get; set; }
}
