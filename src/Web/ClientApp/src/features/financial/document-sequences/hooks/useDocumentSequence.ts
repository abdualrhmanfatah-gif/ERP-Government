import { useQuery } from '@tanstack/react-query';
import { documentSequencesClient } from '../client';

export function useDocumentSequence(id: number | null) {
  return useQuery({
    queryKey: ['document-sequences', id],
    queryFn: () => documentSequencesClient.getById(id!),
    enabled: id !== null,
  });
}
