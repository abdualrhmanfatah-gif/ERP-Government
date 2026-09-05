using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class Unit : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? UnitType { get; set; }
    public int? BaseUnitId { get; set; }
    public decimal? ConversionToBase { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Unit? BaseUnit { get; set; }
}
