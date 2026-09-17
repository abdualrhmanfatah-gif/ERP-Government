import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { partiesClient, partiesKeys } from '../shared/client';
import type { CreatePartyCommand, UpdatePartyCommand, PartyFilters } from '../shared/types';

export function usePartiesList(filters?: PartyFilters) {
  return useQuery({
    queryKey: partiesKeys.list(filters),
    queryFn: () => partiesClient.list(filters),
  });
}

export function useParty(id: number) {
  return useQuery({
    queryKey: partiesKeys.detail(id),
    queryFn: () => partiesClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function usePartyDocuments(id: number) {
  return useQuery({
    queryKey: partiesKeys.documents(id),
    queryFn: () => partiesClient.getDocuments(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateParty() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreatePartyCommand) => partiesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: partiesKeys.all }),
  });
}

export function useUpdateParty(id: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdatePartyCommand) => partiesClient.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: partiesKeys.detail(id) });
      qc.invalidateQueries({ queryKey: partiesKeys.lists() });
    },
  });
}

export function useTogglePartyActive(id: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (targetId?: number) => partiesClient.toggleActive(targetId ?? id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: partiesKeys.detail(id) });
      qc.invalidateQueries({ queryKey: partiesKeys.lists() });
    },
  });
}
