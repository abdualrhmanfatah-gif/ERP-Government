import { useQuery } from '@tanstack/react-query';
import { closingEntriesClient } from '../client';
import type { ClosingEntryDto } from '../types';

export function useClosingEntryById(id: number) {
  return useQuery<ClosingEntryDto>({
    queryKey: ['closingEntry', id],
    queryFn: () => closingEntriesClient.getById(id),
    enabled: id > 0,
  });
}
