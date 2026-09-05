using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Common.DTOs;

// T-017-005 — ClosingEntryDto
public class ClosingEntryDto
{
    public int Id { get; init; }
    public string ClosingEntryNumber { get; init; } = string.Empty;
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public DateOnly ClosingDate { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsReversal { get; init; }
    public int? ReversalOfId { get; init; }
    public string? ReversalOfNumber { get; init; }
    public int? JournalEntryId { get; init; }
    public string? JournalEntryEntryNumber { get; init; }
    public string? ApprovedById { get; init; }
    public bool IsActive { get; init; }
    public string? RowVersion { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<YearEndClosingEntry, ClosingEntryDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.FiscalYearName, opt => opt.MapFrom(s => s.FiscalYear.Name))
                .ForMember(d => d.ReversalOfNumber, opt => opt.MapFrom(s => s.ReversalOf != null ? s.ReversalOf.ClosingEntryNumber : null))
                .ForMember(d => d.RowVersion, opt => opt.MapFrom(s => Convert.ToBase64String(s.RowVersion)));
        }
    }
}
