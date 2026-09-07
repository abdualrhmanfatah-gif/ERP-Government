import { useQuery } from '@tanstack/react-query';
import { JournalsClient } from '../../../web-api-client';

const client = new JournalsClient();

export function useJournalById(id: number | null) {
  return useQuery({
    queryKey: ['journal', id],
    queryFn: () => client.journalsGET(id!),
    enabled: id !== null && id > 0,
  });
}
