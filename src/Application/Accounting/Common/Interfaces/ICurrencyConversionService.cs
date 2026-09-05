using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.Accounting.Common.Interfaces;

public interface ICurrencyConversionService
{
    Task<JournalEntryLineConversion> ConvertAtPostAsync(
        JournalEntryLine line,
        int baseCurrencyId,
        DateOnly effectiveDate,
        ExchangeRateType rateType = ExchangeRateType.Official,
        CancellationToken ct = default);
}

public sealed record JournalEntryLineConversion(
    decimal BaseDebit,
    decimal BaseCredit,
    decimal? Rate,
    DateOnly? ResolvedRateDate,
    bool IsBaseCurrency);
