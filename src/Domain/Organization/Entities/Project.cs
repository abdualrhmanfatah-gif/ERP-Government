using ERP_Government.Domain.Common;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Domain.Organization.Entities;

public class Project : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? FundId { get; set; }
    public int? CostCenterId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? BudgetAmount { get; set; }
    public ProjectStatus Status { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public CostCenter? CostCenter { get; set; }
}
