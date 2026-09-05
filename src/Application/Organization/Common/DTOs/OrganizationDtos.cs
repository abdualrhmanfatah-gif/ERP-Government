using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Application.Organization.Common.DTOs;

// T-O001 — OrganizationalUnitDto
public class OrganizationalUnitDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public string? ParentPath { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<OrganizationalUnit, OrganizationalUnitDto>();
        }
    }
}

// T-O002 — EmployeeDto
public class EmployeeDto
{
    public int Id { get; init; }
    public string EmployeeNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public int OrganizationalUnitId { get; init; }
    public string OrganizationalUnitName { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string? JobGrade { get; init; }
    public DateOnly HireDate { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Employee, EmployeeDto>()
                .ForMember(d => d.OrganizationalUnitName, opt => opt.MapFrom(s => s.OrganizationalUnit.Name))
                .ForMember(d => d.EmploymentStatus, opt => opt.MapFrom(s => s.EmploymentStatus.ToString()));
        }
    }
}

// T-O003 — CostCenterDto
public class CostCenterDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? OrganizationUnitId { get; init; }
    public string? OrganizationUnitName { get; init; }
    public decimal? BudgetLimit { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CostCenter, CostCenterDto>()
                .ForMember(d => d.OrganizationUnitName, opt => opt.MapFrom(s => s.OrganizationUnit != null ? s.OrganizationUnit.Name : null));
        }
    }
}

// T-O004 — ProjectDto
public class ProjectDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? FundId { get; init; }
    public int? CostCenterId { get; init; }
    public string? CostCenterName { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Project, ProjectDto>()
                .ForMember(d => d.CostCenterName, opt => opt.MapFrom(s => s.CostCenter != null ? s.CostCenter.Name : null))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        }
    }
}
