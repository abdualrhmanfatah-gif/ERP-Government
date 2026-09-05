using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Warehouse seed data — 2 warehouses.
/// </summary>
public static class WarehouseSeedData
{
    public static List<Warehouse> GetWarehouses() =>
    [
        new() { Code="WH-001", Name="المخزن الرئيسي", City="صنعاء" },
        new() { Code="WH-002", Name="المخزن الفرعي", City="عدن" },
    ];
}
