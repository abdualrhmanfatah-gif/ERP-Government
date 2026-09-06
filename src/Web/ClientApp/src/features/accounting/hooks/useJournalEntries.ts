import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { journalEntriesClient as client } from '../shared/client';
import type {
  CreateJournalEntryCommand,
  UpdateJournalEntryCommand,
  SubmitJournalEntryCommand,
  ApproveJournalEntryCommand,
  PostJournalEntryCommand,
  ReverseJournalEntryCommand,
  CancelJournalEntryCommand,
} from '../types';

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
}

export interface JournalEntryFilters {
  entryStatus?: string;
  journalId?: number;
  fromDate?: string;
  toDate?: string;
}

export function useJournalEntriesList(filters?: JournalEntryFilters) {
  return useQuery({
    queryKey: ['journalEntries', filters],
    queryFn: () =>
      client.journalEntriesAll(
        filters?.entryStatus,
        filters?.journalId,
        filters?.fromDate ? new Date(filters.fromDate) : undefined,
        filters?.toDate ? new Date(filters.toDate) : undefined,
      ),
  });
}

export function useJournalEntry(id: number) {
  return useQuery({
    queryKey: ['journalEntry', id],
    queryFn: () => client.journalEntriesGET(id),
    enabled: id > 0,
  });
}

export function useCreateJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: CreateJournalEntryCommand) => client.journalEntriesPOST(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
    },
  });
}

export function useUpdateJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command: UpdateJournalEntryCommand }) =>
      client.journalEntriesPUT(id, command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useSubmitJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: SubmitJournalEntryCommand }) =>
      client.submitPOST3(id, command ?? {}),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useApproveJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: ApproveJournalEntryCommand }) =>
      client.approvePOST7(id, command ?? {}),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function usePostJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: PostJournalEntryCommand }) =>
      client.post(id, command ?? {}),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useReverseJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command: ReverseJournalEntryCommand }) =>
      client.reversePOST2(id, command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useCancelJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: CancelJournalEntryCommand }) =>
      client.cancelPOST6(id, command ?? {}),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}
