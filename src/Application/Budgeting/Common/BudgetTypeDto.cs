using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Common;

public class BudgetTypeDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public BudgetControlMethod ControlMethod { get; init; }
    public bool AllowOverrun { get; init; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BudgetType, BudgetTypeDto>();
        }
    }
}
