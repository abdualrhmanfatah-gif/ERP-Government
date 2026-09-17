using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Location seed data — 4 locations (1 root + 3 children).
/// </summary>
public static class LocationSeedData
{
    public static List<Location> GetLocations() =>
    [
        new() { Code="HQ", Name="الإدارة العامة", Level=0, Breadcrumb=null },
        new() { Code="FIN", Name="الإدارة المالية", Level=1, Breadcrumb="HQ" },
        new() { Code="PENSION", Name="إدارة المعاشات", Level=1, Breadcrumb="HQ" },
        new() { Code="LEGAL", Name="إدارة الشؤون القانونية", Level=1, Breadcrumb="HQ" },
    ];

    public static IReadOnlyDictionary<string, string> ParentCodeMap =>
        new Dictionary<string, string>
        {
            ["FIN"] = "HQ",
            ["PENSION"] = "HQ",
            ["LEGAL"] = "HQ",
        };
}
