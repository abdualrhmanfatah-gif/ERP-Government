using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class Location : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int? ParentLocationId { get; set; }
    public int? Level { get; set; }
    public string? Breadcrumb { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public decimal? Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Location? ParentLocation { get; set; }
}
