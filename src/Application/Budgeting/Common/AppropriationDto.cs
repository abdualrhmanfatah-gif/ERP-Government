using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Common;

public class AppropriationDto
{
    public int Id { get; init; }
    public string AppropriationNumber { get; init; } = string.Empty;
    public int BudgetId { get; init; }
    public int BudgetItemId { get; init; }
    public AppropriationType AppropriationType { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public int DocumentId { get; init; }
    public decimal Amount { get; init; }
    public AppropriationStatus Status { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }

    // Contextual FK projections via Budget join
    public string FundNumber { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;
    public string FiscalYearName { get; init; } = string.Empty;
    public string BudgetNumber { get; init; } = string.Empty;
    public string BudgetName { get; init; } = string.Empty;
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Appropriation, AppropriationDto>()
                .ForMember(d => d.FundNumber, opt => opt.MapFrom(s => s.Budget != null && s.Budget.Fund != null ? s.Budget.Fund.FundNumber : string.Empty))
                .ForMember(d => d.FundName, opt => opt.MapFrom(s => s.Budget != null && s.Budget.Fund != null ? s.Budget.Fund.FundName : string.Empty))
                .ForMember(d => d.FiscalYearName, opt => opt.MapFrom(s => s.Budget != null && s.Budget.FiscalYear != null ? s.Budget.FiscalYear.Name : string.Empty))
                .ForMember(d => d.BudgetNumber, opt => opt.MapFrom(s => s.Budget != null ? s.Budget.BudgetNumber : string.Empty))
                .ForMember(d => d.BudgetName, opt => opt.MapFrom(s => s.Budget != null ? s.Budget.BudgetName : string.Empty))
                .ForMember(d => d.ItemCode, opt => opt.MapFrom(s => s.BudgetItem != null ? s.BudgetItem.ItemCode : string.Empty))
                .ForMember(d => d.ItemName, opt => opt.MapFrom(s => s.BudgetItem != null ? s.BudgetItem.ItemName : string.Empty));
        }
    }
}
