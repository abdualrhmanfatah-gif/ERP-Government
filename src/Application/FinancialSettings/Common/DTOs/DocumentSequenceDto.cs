using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Common.DTOs;

public class DocumentSequenceDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DocumentType { get; init; } = string.Empty;
    public int? FiscalYearId { get; init; }
    public int CurrentNumber { get; init; }
    public string ResetPolicy { get; init; } = string.Empty;
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<DocumentSequence, DocumentSequenceDto>()
                .ForMember(d => d.ResetPolicy, opt => opt.MapFrom(s => s.ResetPolicy.ToString()));
        }
    }
}
