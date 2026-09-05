import { useQuery } from '@tanstack/react-query';

interface ExchangeRateLookupDto {
  rate: number;
  rateDate: string;
  rateType: string;
}

export function useExchangeRateLookup(
  baseCurrencyId: number | null,
  currencyId: number | null,
  date: string
) {
  return useQuery({
    queryKey: ['exchangeRate', 'lookup', baseCurrencyId, currencyId, date],
    queryFn: async (): Promise<ExchangeRateLookupDto> => {
      const res = await fetch(
        `/api/ExchangeRates/lookup?BaseCurrencyId=${baseCurrencyId}&CurrencyId=${currencyId}&Date=${date}&RateType=Official`,
        { headers: { Accept: 'application/json' } }
      );
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      return res.json();
    },
    enabled: !!baseCurrencyId && !!currencyId && !!date && currencyId !== baseCurrencyId,
    staleTime: 60_000,
  });
}
