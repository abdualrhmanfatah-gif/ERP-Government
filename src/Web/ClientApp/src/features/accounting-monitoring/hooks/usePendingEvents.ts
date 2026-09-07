import { useQuery } from '@tanstack/react-query';
import { accountingEventsClient } from '../shared/client';

export function usePendingEvents(filters?: {
  status?: string;
  eventType?: string;
}) {
  return useQuery({
    queryKey: ['pendingEvents', filters],
    queryFn: () =>
      accountingEventsClient.pending(
        filters?.status,
        filters?.eventType,
      ),
  });
}
