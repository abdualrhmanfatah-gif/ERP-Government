using ERP_Government.Application.Accounting.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.Accounting.Common.Services;

public class CurrencyConversionService(IExchangeRateResolver rateResolver) : ICurrencyConversionService
{
    public async Task<JournalEntryLineConversion> ConvertAtPostAsync(
        JournalEntryLine line,
        int baseCurrencyId,
        DateOnly effectiveDate,
        ExchangeRateType rateType = ExchangeRateType.Official,
        CancellationToken ct = default)
    {
        if (line.CurrencyId == baseCurrencyId)
            return new JournalEntryLineConversion(line.Debit, line.Credit, 1m, effectiveDate, true);

        var rate = await rateResolver.GetEffectiveRateAsync(
            baseCurrencyId, line.CurrencyId, effectiveDate, rateType, ct);

        // No applicable rate (incl. no prior rate) → signal no-rate for caller to BLOCK (DEC-001).
        if (rate is null)
            return new JournalEntryLineConversion(0m, 0m, null, null, false);

        var baseDebit = Math.Round(line.Debit * rate.Rate, 2);
        var baseCredit = Math.Round(line.Credit * rate.Rate, 2);

        return new JournalEntryLineConversion(baseDebit, baseCredit, rate.Rate, rate.ResolvedRateDate, false);
    }
}
