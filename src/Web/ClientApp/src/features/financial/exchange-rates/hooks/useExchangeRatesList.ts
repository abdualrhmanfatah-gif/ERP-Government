import { useQuery } from '@tanstack/react-query';
import { ExchangeRatesClient } from '../../../../web-api-client';

const client = new ExchangeRatesClient();

export interface ExchangeRateFilters {
  currencyId?: number;
  rateType?: number;
  fromDate?: Date;
  toDate?: Date;
  isActive?: boolean;
}

export function useExchangeRatesList(filters?: ExchangeRateFilters) {
  return useQuery({
    queryKey: ['exchangeRates', filters],
    queryFn: () =>
      client.exchangeRatesAll(
        filters?.currencyId,
        filters?.rateType,
        filters?.fromDate,
        filters?.toDate,
        filters?.isActive,
      ),
  });
}