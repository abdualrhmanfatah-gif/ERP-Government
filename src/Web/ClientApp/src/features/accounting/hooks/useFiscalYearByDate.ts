import { useQuery } from '@tanstack/react-query';
import { fiscalYearsClient } from '../../financial-settings/shared/client';

export function useFiscalYearByDate(date: string) {
  return useQuery({
    queryKey: ['fiscalYear', 'by-date', date],
    queryFn: () => fiscalYearsClient.byDate(date),
    enabled: !!date,
    staleTime: 60_000,
  });
}
