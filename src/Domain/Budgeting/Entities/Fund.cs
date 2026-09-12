using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class Fund : BaseAuditableEntity
{
    public string FundNumber { get; set; } = string.Empty;
    public string FundName { get; set; } = string.Empty;
    public Enums.FundType FundType { get; set; }
    public Enums.FundCategory FundCategory { get; set; }
    public string LegalAuthority { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DefaultRevenueAccountId { get; set; }
    public int? CurrencyId { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
