import { useMutation, useQueryClient } from '@tanstack/react-query';
import { journalEntriesClient as client } from '../shared/client';
import type { CreateJournalEntryLineCommand, UpdateJournalEntryLineCommand } from '../types';

export function useCreateJournalEntryLine() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      journalEntryId,
      command,
    }: {
      journalEntryId: number;
      command: CreateJournalEntryLineCommand;
    }) => client.linesPOST3(journalEntryId, command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['journalEntry', variables.journalEntryId],
      });
    },
  });
}

export function useUpdateJournalEntryLine() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      journalEntryId,
      lineId,
      command,
    }: {
      journalEntryId: number;
      lineId: number;
      command: UpdateJournalEntryLineCommand;
    }) => client.linesPUT(journalEntryId, lineId, command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['journalEntry', variables.journalEntryId],
      });
    },
  });
}

export function useRemoveJournalEntryLine() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      journalEntryId,
      lineId,
    }: {
      journalEntryId: number;
      lineId: number;
    }) => client.linesDELETE(journalEntryId, lineId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['journalEntry', variables.journalEntryId],
      });
    },
  });
}
