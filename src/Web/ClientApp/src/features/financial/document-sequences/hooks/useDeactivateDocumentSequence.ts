import { useMutation, useQueryClient } from '@tanstack/react-query';
import { documentSequencesClient } from '../client';

export function useDeactivateDocumentSequence() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => documentSequencesClient.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['document-sequences'] }),
  });
}
