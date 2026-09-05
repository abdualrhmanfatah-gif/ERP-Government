using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Employee seed data — 5 employees.
/// </summary>
public static class EmployeeSeedData
{
    public static List<Employee> GetEmployees() =>
    [
        new() { EmployeeNumber="EMP-001", Name="أحمد محمد", OrganizationalUnitId=1, JobTitle="مدير عام", HireDate=new DateOnly(2020,1,1), EmploymentStatus=EmploymentStatus.Active },
        new() { EmployeeNumber="EMP-002", Name="فاطمة علي", OrganizationalUnitId=2, JobTitle="محاسب", HireDate=new DateOnly(2021,3,15), EmploymentStatus=EmploymentStatus.Active },
        new() { EmployeeNumber="EMP-003", Name="محمد حسن", OrganizationalUnitId=3, JobTitle="موظف مشتريات", HireDate=new DateOnly(2022,6,1), EmploymentStatus=EmploymentStatus.Active },
        new() { EmployeeNumber="EMP-004", Name="نورا أحمد", OrganizationalUnitId=4, JobTitle="موظف موارد بشرية", HireDate=new DateOnly(2023,1,10), EmploymentStatus=EmploymentStatus.Active },
        new() { EmployeeNumber="EMP-005", Name="خالد عبدالله", OrganizationalUnitId=5, JobTitle="أمين مخزن", HireDate=new DateOnly(2021,9,1), EmploymentStatus=EmploymentStatus.Active },
    ];
}
