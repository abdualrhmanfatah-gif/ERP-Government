import { useQuery } from '@tanstack/react-query';
import { closingEntriesClient } from '../client';
import type { ClosingEntryDto } from '../types';

export function useClosingEntriesByFiscalYear(fiscalYearId: number) {
  return useQuery<ClosingEntryDto[]>({
    queryKey: ['closingEntries', fiscalYearId],
    queryFn: () => closingEntriesClient.listByFiscalYear(fiscalYearId),
    enabled: fiscalYearId > 0,
  });
}
