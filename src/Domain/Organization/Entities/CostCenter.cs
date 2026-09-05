using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Organization.Entities;

public class CostCenter : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? OrganizationUnitId { get; set; }
    public decimal? BudgetLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public OrganizationalUnit? OrganizationUnit { get; set; }
}
