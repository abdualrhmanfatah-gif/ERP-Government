import { useQuery } from '@tanstack/react-query';
import { JournalsClient, JournalType } from '../../../web-api-client';

const client = new JournalsClient();

export function useJournalsList(filters?: { isActive?: boolean; type?: JournalType }) {
  return useQuery({
    queryKey: ['journals', filters],
    queryFn: () => client.journalsAll(filters?.isActive, filters?.type ?? null),
  });
}
