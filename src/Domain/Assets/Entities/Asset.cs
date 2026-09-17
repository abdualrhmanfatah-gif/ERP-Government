using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Domain.Assets.Entities;

public class Asset : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssetGroupId { get; set; }
    public int? LocationId { get; set; }
    public int? EmployeeId { get; set; }
    public int CurrencyId { get; set; }
    public int? ExchangeRateId { get; set; }
    public string? AssetTag { get; set; }
    public string? Barcode { get; set; }
    public string? SerialNumber { get; set; }
    public decimal OriginalValue { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal? CurrentValue { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? ActivationDate { get; set; }
    public DateOnly? DepreciationStartDate { get; set; }
    public DateOnly? LastDepreciationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AcquisitionType { get; set; } = string.Empty;
    public int? UsefulLifeYears { get; set; }
    public bool IsFullyDepreciated { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public AssetGroup? AssetGroup { get; set; }
    public Location? Location { get; set; }
    public Employee? Employee { get; set; }
    public Currency? Currency { get; set; }
    public ExchangeRate? ExchangeRate { get; set; }
}
