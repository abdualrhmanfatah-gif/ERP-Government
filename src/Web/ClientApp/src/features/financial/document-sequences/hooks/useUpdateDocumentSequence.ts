import { useMutation, useQueryClient } from '@tanstack/react-query';
import { documentSequencesClient } from '../client';
import type { UpdateDocumentSequenceCommand } from '../types';

export function useUpdateDocumentSequence() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateDocumentSequenceCommand) => documentSequencesClient.update(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['document-sequences'] }),
  });
}
