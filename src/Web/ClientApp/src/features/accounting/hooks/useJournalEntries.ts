import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { journalEntriesClient as client } from '../shared/client';
import {
  UpdateJournalEntryCommand,
  SubmitJournalEntryCommand,
  ApproveJournalEntryCommand,
  PostJournalEntryCommand,
  ReverseJournalEntryCommand,
  CancelJournalEntryCommand,
} from '../../../web-api-client';
import type {
  CreateJournalEntryCommand,
  UpdateJournalEntryCommand as UpdateJournalEntryCommandData,
  SubmitJournalEntryCommand as SubmitJournalEntryCommandData,
  ApproveJournalEntryCommand as ApproveJournalEntryCommandData,
  PostJournalEntryCommand as PostJournalEntryCommandData,
  ReverseJournalEntryCommand as ReverseJournalEntryCommandData,
  CancelJournalEntryCommand as CancelJournalEntryCommandData,
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
    mutationFn: ({ id, command }: { id: number; command: UpdateJournalEntryCommandData }) =>
      client.journalEntriesPUT(id, new UpdateJournalEntryCommand({
        id,
        narration: command.narration ?? undefined,
        ref: command.ref ?? undefined,
        rowVersion: command.rowVersion,
      })),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useSubmitJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: SubmitJournalEntryCommandData }) =>
      client.submitPOST3(id, new SubmitJournalEntryCommand({ id, ...command })),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useApproveJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: ApproveJournalEntryCommandData }) =>
      client.approvePOST7(id, new ApproveJournalEntryCommand({ id, ...command })),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function usePostJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: PostJournalEntryCommandData }) =>
      client.post(id, new PostJournalEntryCommand({ id, ...command })),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useReverseJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command: ReverseJournalEntryCommandData }) =>
      client.reversePOST2(id, new ReverseJournalEntryCommand({ id, ...command })),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}

export function useCancelJournalEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, command }: { id: number; command?: CancelJournalEntryCommandData }) =>
      client.cancelPOST6(id, new CancelJournalEntryCommand({ id, ...command })),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['journalEntries'] });
      queryClient.invalidateQueries({ queryKey: ['journalEntry', variables.id] });
    },
  });
}
