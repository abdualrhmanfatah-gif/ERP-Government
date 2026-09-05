import { useQuery } from '@tanstack/react-query';
import { FiscalYearsClient } from '../../../../web-api-client';

const client = new FiscalYearsClient();

export function useFiscalYearsList() {
  return useQuery({
    queryKey: ['fiscalYears'],
    queryFn: () => client.fiscalYearsAll(undefined),
  });
}
