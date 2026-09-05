using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class Warehouse : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public int? ManagerId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal? TotalCapacity { get; set; }
    public decimal? CurrentLoad { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Location? Location { get; set; }
}
