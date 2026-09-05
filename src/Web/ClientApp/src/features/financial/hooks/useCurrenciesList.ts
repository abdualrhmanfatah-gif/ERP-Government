import { useQuery } from '@tanstack/react-query';
import { CurrenciesClient } from '../../../web-api-client';

const client = new CurrenciesClient();

export function useCurrenciesList(filters?: { isActive?: boolean }) {
  return useQuery({
    queryKey: ['currencies', filters],
    queryFn: () => client.currenciesAll(filters?.isActive),
  });
}
