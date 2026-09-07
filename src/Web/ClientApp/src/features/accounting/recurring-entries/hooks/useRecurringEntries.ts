import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { recurringEntriesClient, type RecurringEntryListFilters } from '../shared/client';
import type {
  CreateRecurringEntryCommand,
  PauseRecurringEntryCommand,
  ResumeRecurringEntryCommand,
  CancelRecurringEntryCommand,
} from '../shared/types';

export const recurringEntryKeys = {
  all: ['recurringEntries'] as const,
  lists: () => [...recurringEntryKeys.all, 'list'] as const,
  list: (filters?: RecurringEntryListFilters) => [...recurringEntryKeys.lists(), filters] as const,
  details: () => [...recurringEntryKeys.all, 'detail'] as const,
  detail: (id: number) => [...recurringEntryKeys.details(), id] as const,
};

export function useRecurringEntries(filters?: RecurringEntryListFilters) {
  return useQuery({
    queryKey: recurringEntryKeys.list(filters),
    queryFn: () =>
      recurringEntriesClient.recurringEntriesAll(
        filters?.isActive,
        filters?.journalId,
        filters?.frequency as string | undefined,
        filters?.status as string | undefined,
      ),
  });
}

export function useRecurringEntry(id: number) {
  return useQuery({
    queryKey: recurringEntryKeys.detail(id),
    queryFn: () => recurringEntriesClient.recurringEntriesGET(id),
    enabled: id > 0,
  });
}

export function useCreateRecurringEntry() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRecurringEntryCommand) =>
      recurringEntriesClient.recurringEntriesPOST(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: recurringEntryKeys.lists() });
    },
  });
}

export function usePauseRecurringEntry() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: PauseRecurringEntryCommand }) =>
      recurringEntriesClient.recurringEntriesPOST2(id, data),
    onSuccess: (_result, variables) => {
      queryClient.invalidateQueries({ queryKey: recurringEntryKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: recurringEntryKeys.lists() });
    },
  });
}

export function useResumeRecurringEntry() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: ResumeRecurringEntryCommand }) =>
      recurringEntriesClient.recurringEntriesPOST3(id, data),
    onSuccess: (_result, variables) => {
      queryClient.invalidateQueries({ queryKey: recurringEntryKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: recurringEntryKeys.lists() });
    },
  });
}

export function useCancelRecurringEntry() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: CancelRecurringEntryCommand }) =>
      recurringEntriesClient.recurringEntriesPOST4(id, data),
    onSuccess: (_result, variables) => {
      queryClient.invalidateQueries({ queryKey: recurringEntryKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: recurringEntryKeys.lists() });
    },
  });
}
