using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Project seed data — 2 projects.
/// </summary>
public static class ProjectSeedData
{
    public static List<Project> GetProjects() =>
    [
        new() { Code="PRJ-001", Name="مشروع تحديث التقاعد", FundId=1, CostCenterId=5, StartDate=new DateOnly(2026,1,1), EndDate=new DateOnly(2026,12,31), BudgetAmount=20000000, Status=ProjectStatus.Active },
        new() { Code="PRJ-002", Name="مشروع الاستثمار", FundId=2, CostCenterId=5, StartDate=new DateOnly(2026,1,1), EndDate=new DateOnly(2026,12,31), BudgetAmount=50000000, Status=ProjectStatus.Active },
    ];
}
