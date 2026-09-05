using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

// FEATURE-008 — RolePermission junction entity
public class RolePermission : BaseAuditableEntity
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public SecurityRole Role { get; set; } = null!;
    public SecurityPermission Permission { get; set; } = null!;
}
