using ERP_Government.Domain.Budgeting.Entities;

namespace ERP_Government.Application.Budgeting.Common;

public class BudgetItemDto
{
    public int Id { get; init; }
    public int BudgetId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public string? Remarks { get; init; }
    public int Level { get; set; }
    public bool? AllowOverrun { get; init; }
    public bool AllowOverrunEffective { get; set; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }

    // Tree children (for tree query)
    public List<BudgetItemDto> Children { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BudgetItem, BudgetItemDto>()
                .ForMember(d => d.Level, opt => opt.Ignore())
                .ForMember(d => d.AllowOverrunEffective, opt => opt.Ignore())
                .ForMember(d => d.Children, opt => opt.Ignore());
        }
    }
}
