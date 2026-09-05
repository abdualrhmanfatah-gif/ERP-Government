import { useQuery } from '@tanstack/react-query';
import { FiscalPeriodsClient } from '../../../../web-api-client';

const client = new FiscalPeriodsClient();

export function useFiscalPeriodsList(fiscalYearId: number) {
  return useQuery({
    queryKey: ['fiscalPeriods', fiscalYearId],
    queryFn: () => client.fiscalPeriodsAll(fiscalYearId),
    enabled: !!fiscalYearId,
  });
}
