using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Common;

public class FundDto
{
    public int Id { get; init; }
    public string FundNumber { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;
    public FundType FundType { get; init; }
    public FundCategory FundCategory { get; init; }
    public int? FiscalYearId { get; init; }
    public string LegalAuthority { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? DefaultRevenueDebitAccountId { get; init; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Fund, FundDto>();
        }
    }
}
