using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Item seed data — 3 items.
/// </summary>
public static class ItemSeedData
{
    public static List<Item> GetItems() =>
    [
        new() { CategoryId=1, Code="ITM-001", Name="أقلام حبر", UnitId=1, ItemType="Consumable" },
        new() { CategoryId=2, Code="ITM-002", Name="كراسي مكتب", UnitId=1, ItemType="Product" },
        new() { CategoryId=3, Code="ITM-003", Name="إطارات سيارات", UnitId=1, ItemType="Product" },
    ];
}
