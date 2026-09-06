import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { financialSettingsKeys, documentSequencesClient } from '../shared/client';
import type { CreateDocumentSequenceCommand, UpdateDocumentSequenceCommand } from '../shared/types';

export function useDocumentSequencesList(isActive?: boolean) {
  return useQuery({
    queryKey: financialSettingsKeys.documentSequences.list({ isActive }),
    queryFn: () => documentSequencesClient.list(isActive),
  });
}

export function useDocumentSequenceDetail(id: number) {
  return useQuery({
    queryKey: financialSettingsKeys.documentSequences.detail(id),
    queryFn: () => documentSequencesClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateDocumentSequence() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDocumentSequenceCommand) => documentSequencesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.documentSequences.all }),
  });
}

export function useUpdateDocumentSequence() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: UpdateDocumentSequenceCommand) =>
      documentSequencesClient.update(id, { id, ...data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.documentSequences.all }),
  });
}

export function useDeactivateDocumentSequence() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      documentSequencesClient.deactivate(id, rowVersion),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.documentSequences.all }),
  });
}
