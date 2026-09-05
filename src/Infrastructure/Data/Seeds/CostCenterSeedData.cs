using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// CostCenter seed data — 5 centers.
/// </summary>
public static class CostCenterSeedData
{
    public static List<CostCenter> GetCostCenters() =>
    [
        new() { Code="CC-FIN", Name="مركز تكلفة المالية", OrganizationUnitId=2, BudgetLimit=5000000 },
        new() { Code="CC-PROC", Name="مركز تكلفة المشتريات", OrganizationUnitId=3, BudgetLimit=10000000 },
        new() { Code="CC-HR", Name="مركز تكلفة الموارد البشرية", OrganizationUnitId=4, BudgetLimit=3000000 },
        new() { Code="CC-WH", Name="مركز تكلفة المخازن", OrganizationUnitId=5, BudgetLimit=2000000 },
        new() { Code="CC-ADMIN", Name="مركز تكلفة الإدارة العامة", OrganizationUnitId=1, BudgetLimit=8000000 },
    ];
}
