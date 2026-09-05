using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetPhysicalCount : BaseAuditableEntity
{
    public string CountNumber { get; set; } = string.Empty;
    public DateOnly CountDate { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public string CountType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CountedById { get; set; }
    public int? ReviewedById { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
