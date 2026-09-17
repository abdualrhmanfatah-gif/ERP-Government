using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetPhysicalCountDetail : BaseAuditableEntity
{
    public int AssetPhysicalCountId { get; set; }
    public int AssetId { get; set; }
    public int? SystemLocationId { get; set; }
    public int? PhysicalLocationId { get; set; }
    public int? SystemEmployeeId { get; set; }
    public int? PhysicalEmployeeId { get; set; }
    public string? SystemStatus { get; set; }
    public string? PhysicalStatus { get; set; }
    public CountFoundState IsFound { get; set; } = CountFoundState.NotExamined;
    public bool? IsMatch { get; set; }
    public string? DiscrepancyNotes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public AssetPhysicalCount? AssetPhysicalCount { get; set; }
    public Asset? Asset { get; set; }
    public Location? SystemLocation { get; set; }
    public Location? PhysicalLocation { get; set; }
    public Employee? SystemEmployee { get; set; }
    public Employee? PhysicalEmployee { get; set; }
}
