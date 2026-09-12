using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class BudgetClassification : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int? ClassificationLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
