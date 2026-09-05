using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class FieldSecurityPolicy : BaseAuditableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public string? MaskingFormat { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public SecurityRole Role { get; set; } = null!;
}
