import { useQuery } from '@tanstack/react-query';
import { CurrenciesClient } from '../../../web-api-client';

const client = new CurrenciesClient();

export function useCurrencyDetail(id: number | null) {
  return useQuery({
    queryKey: ['currency', id],
    queryFn: () => client.currenciesGET(id!),
    enabled: id !== null && id > 0,
  });
}
