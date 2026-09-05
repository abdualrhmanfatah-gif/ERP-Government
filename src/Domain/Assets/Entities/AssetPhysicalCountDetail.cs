using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetPhysicalCountDetail : BaseAuditableEntity
{
    public int AssetPhysicalCountId { get; set; }
    public int AssetId { get; set; }
    public int? SystemLocationId { get; set; }
    public int? PhysicalLocationId { get; set; }
    public int? SystemCustodianId { get; set; }
    public int? PhysicalCustodianId { get; set; }
    public string? SystemStatus { get; set; }
    public string? PhysicalStatus { get; set; }
    public bool IsFound { get; set; }
    public bool IsMatch { get; set; }
    public string? DiscrepancyNotes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public AssetPhysicalCount? AssetPhysicalCount { get; set; }
    public Asset? Asset { get; set; }
}
