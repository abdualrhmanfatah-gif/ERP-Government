using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.FinancialSettings.Entities;

public class Currency : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public decimal RoundingPrecision { get; set; } = 0.01m;
    public bool IsBase { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
