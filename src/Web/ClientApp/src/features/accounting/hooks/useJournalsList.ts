import { useQuery } from '@tanstack/react-query';
import { JournalsClient } from '../../../web-api-client';

const client = new JournalsClient();

export function useJournalsList(filters?: { isActive?: boolean }) {
  return useQuery({
    queryKey: ['journals', filters],
    queryFn: () => client.journalsAll(filters?.isActive, undefined),
  });
}
