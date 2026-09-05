using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// OrganizationalUnit seed data — 5 units.
/// </summary>
public static class OrganizationalUnitSeedData
{
    public static List<OrganizationalUnit> GetUnits() =>
    [
        new() { Code="HQ", Name="الإدارة العامة", ParentId=null, ParentPath="/1" },
        new() { Code="FIN", Name="قسم المالية", ParentId=1, ParentPath="/1/2" },
        new() { Code="PROC", Name="قسم المشتريات", ParentId=1, ParentPath="/1/3" },
        new() { Code="HR", Name="قسم الموارد البشرية", ParentId=1, ParentPath="/1/4" },
        new() { Code="WH", Name="قسم المخازن", ParentId=1, ParentPath="/1/5" },
    ];
}
