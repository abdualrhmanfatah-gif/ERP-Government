using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Common;

public class BudgetDto
{
    public int Id { get; init; }
    public string BudgetNumber { get; init; } = string.Empty;
    public string BudgetName { get; init; } = string.Empty;
    public int FiscalYearId { get; init; }
    public int FundId { get; init; }
    public int BudgetTypeId { get; init; }
    public BudgetStatus Status { get; init; }
    public bool? AllowOverrun { get; init; }
    public decimal TotalAmount { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public string? Description { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }

    // FK name projections
    public string BudgetTypeName { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Budget, BudgetDto>()
                .ForMember(d => d.BudgetTypeName, opt => opt.MapFrom(s => s.BudgetType != null ? s.BudgetType.Name : string.Empty))
                .ForMember(d => d.FundName, opt => opt.MapFrom(s => s.Fund != null ? s.Fund.FundName : string.Empty));
        }
    }
}
