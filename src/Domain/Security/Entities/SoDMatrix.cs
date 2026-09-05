using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class SoDMatrix : BaseAuditableEntity
{
    public int PermissionAId { get; set; }
    public int PermissionBId { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public ActionOnViolation ActionOnViolation { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public SecurityPermission PermissionA { get; set; } = null!;
    public SecurityPermission PermissionB { get; set; } = null!;
}
