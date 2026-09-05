using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Location seed data — 6 locations (3 per warehouse).
/// </summary>
public static class LocationSeedData
{
    public static List<Location> GetLocations() =>
    [
        new() { Code="LOC-RECV-1", Name="منطقة الاستلام - صنعاء", City="صنعاء" },
        new() { Code="LOC-STORE-1", Name="منطقة التخزين - صنعاء", City="صنعاء" },
        new() { Code="LOC-SHIP-1", Name="منطقة الشحن - صنعاء", City="صنعاء" },
        new() { Code="LOC-RECV-2", Name="منطقة الاستلام - عدن", City="عدن" },
        new() { Code="LOC-STORE-2", Name="منطقة التخزين - عدن", City="عدن" },
        new() { Code="LOC-SHIP-2", Name="منطقة الشحن - عدن", City="عدن" },
    ];
}
