using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Unit seed data — 5 units.
/// </summary>
public static class UnitSeedData
{
    public static List<Unit> GetUnits() =>
    [
        new() { Code="PC", Name="Piece", NameAr="قطعة" },
        new() { Code="KG", Name="Kilogram", NameAr="كجم" },
        new() { Code="M", Name="Meter", NameAr="متر" },
        new() { Code="L", Name="Liter", NameAr="لتر" },
        new() { Code="BOX", Name="Box", NameAr="علبة" },
    ];
}
