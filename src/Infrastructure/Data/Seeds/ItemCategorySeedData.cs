using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// ItemCategory seed data — 3 categories.
/// </summary>
public static class ItemCategorySeedData
{
    public static List<ItemCategory> GetItemCategories() =>
    [
        new() { Code="CAT-001", Name="قرطاسية" },
        new() { Code="CAT-002", Name="أثاث ومعدات" },
        new() { Code="CAT-003", Name="قطع غيار" },
    ];
}
