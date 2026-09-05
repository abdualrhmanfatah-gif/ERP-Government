import { useMutation, useQueryClient } from '@tanstack/react-query';
import { documentSequencesClient } from '../client';
import type { CreateDocumentSequenceCommand } from '../types';

export function useCreateDocumentSequence() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDocumentSequenceCommand) => documentSequencesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['document-sequences'] }),
  });
}
