using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class FinalAccountLine : BaseEntity
{
    public int FinalAccountId { get; set; }
    public Enums.FinalAccountLineDimension Dimension { get; set; }
    public int DimensionId { get; set; }
    public string DimensionCode { get; set; } = string.Empty;
    public string DimensionName { get; set; } = string.Empty;
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public FinalAccount FinalAccount { get; set; } = null!;
}
