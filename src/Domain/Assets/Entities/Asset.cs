using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class Asset : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssetGroupId { get; set; }
    public int? LocationId { get; set; }
    public int? FundId { get; set; }
    public int? CostCenterId { get; set; }
    public int? CustodianId { get; set; }
    public string? AssetTag { get; set; }
    public string? Barcode { get; set; }
    public string? SerialNumber { get; set; }
    public string? ImageUrl { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal OriginalValue { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public decimal? ResidualValue { get; set; }
    public decimal? RelinquishmentValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal? CurrentValue { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public DateOnly? ActivationDate { get; set; }
    public DateOnly DepreciationStartDate { get; set; }
    public DateOnly? LastDepreciationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AcquisitionType { get; set; } = string.Empty;
    public int? UsefulLifeYears { get; set; }
    public bool IsFullyDepreciated { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public AssetGroup? AssetGroup { get; set; }
}
