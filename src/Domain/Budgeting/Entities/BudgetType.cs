using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class BudgetType : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Enums.BudgetControlMethod ControlMethod { get; set; }
    public bool AllowOverrun { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
