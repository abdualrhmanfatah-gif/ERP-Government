using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Organization.Entities;

public class OrganizationalUnit : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? ParentPath { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public OrganizationalUnit? Parent { get; set; }
}
