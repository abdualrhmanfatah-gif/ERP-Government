using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class BudgetItem : BaseAuditableEntity
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int BudgetId { get; set; }
    public int? ParentId { get; set; }
    public int? AccountId { get; set; }
    public int? FundId { get; set; }
    public int? CostCenterId { get; set; }
    public int? BudgetClassificationId { get; set; }
    public string? Remarks { get; set; }
    public bool? AllowOverrun { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Budget Budget { get; set; } = null!;
}
