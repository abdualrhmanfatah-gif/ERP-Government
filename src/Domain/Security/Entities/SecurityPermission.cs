using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class SecurityPermission : BaseAuditableEntity
{
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PermissionLevel PermissionLevel { get; set; }
    public bool IsSensitive { get; set; }
    public DataScope DataScope { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
