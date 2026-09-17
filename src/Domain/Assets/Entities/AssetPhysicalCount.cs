using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetPhysicalCount : BaseAuditableEntity
{
    public string CountNumber { get; set; } = string.Empty;
    public DateOnly CountDate { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public string ResolvedScopeLabel { get; set; } = string.Empty;
    public string CountType { get; set; } = string.Empty;
    public CountStatus Status { get; set; } = CountStatus.Draft;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CountedById { get; set; }
    public int? ReviewedById { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Location? Location { get; set; }
    public OrganizationalUnit? Department { get; set; }
    public User? CountedBy { get; set; }
    public User? ReviewedBy { get; set; }
}
