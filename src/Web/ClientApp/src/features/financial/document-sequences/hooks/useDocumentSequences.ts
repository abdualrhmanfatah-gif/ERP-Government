import { useQuery } from '@tanstack/react-query';
import { documentSequencesClient } from '../client';

export function useDocumentSequences(filters?: { isActive?: boolean }) {
  return useQuery({
    queryKey: ['document-sequences', filters],
    queryFn: () => documentSequencesClient.list(filters?.isActive),
  });
}
