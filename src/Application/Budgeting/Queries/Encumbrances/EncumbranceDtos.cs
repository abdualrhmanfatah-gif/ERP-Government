using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.Encumbrances;

public class EncumbranceListItemDto
{
    public int Id { get; init; }
    public string EncumbranceNumber { get; init; } = string.Empty;
    public EncumbranceType EncumbranceType { get; init; }
    public string? Description { get; init; }
    public DateOnly EncumbranceDate { get; init; }
    public decimal TotalAmount { get; init; }
    public EncumbranceStatus Status { get; init; }
    public bool IsReversed { get; set; }
    public int? ReversalOfId { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
}

public class EncumbranceDetailDto : EncumbranceListItemDto
{
    public int? VendorPartyId { get; init; }
    public int? PurchaseOrderId { get; init; }
    public string? DocumentType { get; init; }
    public int? DocumentId { get; init; }
    public string? ReversalReason { get; init; }
    public List<EncumbranceLineDto> Lines { get; set; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Encumbrance, EncumbranceListItemDto>()
                .ForMember(d => d.IsReversed, opt => opt.MapFrom(s => false));

            CreateMap<Encumbrance, EncumbranceDetailDto>()
                .ForMember(d => d.IsReversed, opt => opt.MapFrom(s => false));
        }
    }
}
