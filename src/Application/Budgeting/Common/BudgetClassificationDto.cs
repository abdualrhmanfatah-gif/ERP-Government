using ERP_Government.Domain.Budgeting.Entities;

namespace ERP_Government.Application.Budgeting.Common;

public class BudgetClassificationDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public int Level { get; set; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public List<BudgetClassificationDto> Children { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BudgetClassification, BudgetClassificationDto>()
                .ForMember(d => d.Level, opt => opt.Ignore())
                .ForMember(d => d.Children, opt => opt.Ignore());
        }
    }
}
