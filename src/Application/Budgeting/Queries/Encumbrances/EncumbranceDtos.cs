using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.Encumbrances;

public class EncumbranceListItemDto
{
    public int Id { get; init; }
    public string EncumbranceNumber { get; init; } = string.Empty;
    public int AppropriationId { get; init; }
    public EncumbranceType EncumbranceType { get; init; }
    public string? Description { get; init; }
    public DateOnly EncumbranceDate { get; init; }
    public decimal Amount { get; init; }
    public EncumbranceStatus Status { get; init; }
    public bool IsReversed { get; set; }
    public int? ReversalOfId { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
}

public class EncumbranceDetailDto : EncumbranceListItemDto
{
    public int? VendorId { get; init; }
    public int? PurchaseOrderId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public int DocumentId { get; init; }
    public string? ReversalReason { get; init; }
    public string AppropriationNumber { get; init; } = string.Empty;
    public string ItemCode { get; init; } = string.Empty;
    public string FundNumber { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Encumbrance, EncumbranceListItemDto>()
                .ForMember(d => d.IsReversed, opt => opt.MapFrom(s => false));

            CreateMap<Encumbrance, EncumbranceDetailDto>()
                .ForMember(d => d.IsReversed, opt => opt.MapFrom(s => false))
                .ForMember(d => d.AppropriationNumber, opt => opt.MapFrom(s => s.Appropriation != null ? s.Appropriation.AppropriationNumber : string.Empty))
                .ForMember(d => d.ItemCode, opt => opt.MapFrom(s => s.Appropriation != null && s.Appropriation.BudgetItem != null ? s.Appropriation.BudgetItem.ItemCode : string.Empty))
                .ForMember(d => d.FundNumber, opt => opt.MapFrom(s => s.Appropriation != null && s.Appropriation.BudgetItem != null && s.Appropriation.BudgetItem.Budget != null && s.Appropriation.BudgetItem.Budget.Fund != null ? s.Appropriation.BudgetItem.Budget.Fund.FundNumber : string.Empty));
        }
    }
}
