using ERP_Government.Domain.Common;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Domain.Organization.Entities;

public class Employee : BaseAuditableEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int OrganizationalUnitId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string? JobGrade { get; set; }
    public DateOnly HireDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public OrganizationalUnit OrganizationalUnit { get; set; } = null!;
}
