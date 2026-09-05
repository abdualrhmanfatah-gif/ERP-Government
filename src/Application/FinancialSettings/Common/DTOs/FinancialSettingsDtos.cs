using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Common.DTOs;

// T-F001 — CurrencyDto
public class CurrencyDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public int DecimalPlaces { get; init; }
    public decimal RoundingPrecision { get; init; }
    public bool IsBase { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Currency, CurrencyDto>();
        }
    }
}

// T-F002 — FiscalYearDto
public class FiscalYearDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int YearNumber { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsClosed { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<FiscalYear, FiscalYearDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        }
    }
}

// T-F003 — FiscalPeriodDto
public class FiscalPeriodDto
{
    public int Id { get; init; }
    public int FiscalYearId { get; init; }
    public int PeriodNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public bool IsLockedForPosting { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<FiscalPeriod, FiscalPeriodDto>();
        }
    }
}

// ExchangeRateDto
public class ExchangeRateDto
{
    public int Id { get; init; }
    public int BaseCurrencyId { get; init; }
    public string BaseCurrencyCode { get; init; } = string.Empty;
    public int CurrencyId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public DateOnly RateDate { get; init; }
    public string RateType { get; init; } = string.Empty;
    public decimal Rate { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset Created { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string? RowVersion { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ExchangeRate, ExchangeRateDto>()
                .ForMember(d => d.BaseCurrencyCode, opt => opt.MapFrom(s => s.BaseCurrency.Code))
                .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Currency.Code))
                .ForMember(d => d.RateType, opt => opt.MapFrom(s => s.RateType.ToString()))
                .ForMember(d => d.RowVersion, opt => opt.MapFrom(s => Convert.ToBase64String(s.RowVersion)));
        }
    }
}

// ExchangeRateLookupDto
public class ExchangeRateLookupDto
{
    public decimal Rate { get; init; }
    public DateOnly RateDate { get; init; }
    public string RateType { get; init; } = string.Empty;
}

// Iso4217CodeDto
public class Iso4217CodeDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int DecimalPlaces { get; init; }
}
