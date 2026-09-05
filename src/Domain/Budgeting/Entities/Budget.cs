using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class Budget : BaseAuditableEntity
{
    public string BudgetNumber { get; set; } = string.Empty;
    public string BudgetName { get; set; } = string.Empty;
    public int BudgetTypeId { get; set; }
    public int FiscalYearId { get; set; }
    public int FundId { get; set; }
    public decimal TotalAmount { get; set; }
    public Enums.BudgetStatus Status { get; set; } = Enums.BudgetStatus.Draft;
    public bool? AllowOverrun { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? Description { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public BudgetType BudgetType { get; set; } = null!;
    public Fund Fund { get; set; } = null!;
    public FinancialSettings.Entities.FiscalYear FiscalYear { get; set; } = null!;
}
