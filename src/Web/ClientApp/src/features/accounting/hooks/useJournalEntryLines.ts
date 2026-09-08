import { useMutation, useQueryClient } from '@tanstack/react-query';
import { journalEntriesClient as client } from '../shared/client';
import { CreateJournalEntryLineCommand, UpdateJournalEntryLineCommand } from '../../../web-api-client';
import type {
  CreateJournalEntryLineCommand as CreateJournalEntryLineData,
  UpdateJournalEntryLineCommand as UpdateJournalEntryLineData,
} from '../types';

export function useCreateJournalEntryLine() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      journalEntryId,
      command,
    }: {
      journalEntryId: number;
      command: CreateJournalEntryLineData;
    }) =>
      client.linesPOST3(
        journalEntryId,
        new CreateJournalEntryLineCommand({
          journalEntryId,
          accountId: command.accountId,
          description: command.description ?? undefined,
          currencyId: command.currencyId,
          exchangeRate: command.exchangeRate,
          debit: command.debit,
          credit: command.credit,
          costCenterId: command.costCenterId ?? undefined,
          fundId: command.fundId ?? undefined,
          projectId: command.projectId ?? undefined,
          budgetItemId: command.budgetItemId ?? undefined,
          encumbranceId: command.encumbranceId ?? undefined,
          paymentOrderId: command.paymentOrderId ?? undefined,
        }),
      ),
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
      command: UpdateJournalEntryLineData;
    }) =>
      client.linesPUT(
        journalEntryId,
        lineId,
        new UpdateJournalEntryLineCommand({
          id: command.id,
          journalEntryId: command.journalEntryId,
          accountId: command.accountId,
          description: command.description ?? undefined,
          currencyId: command.currencyId,
          exchangeRate: command.exchangeRate,
          debit: command.debit,
          credit: command.credit,
          costCenterId: command.costCenterId ?? undefined,
          fundId: command.fundId ?? undefined,
          projectId: command.projectId ?? undefined,
          budgetItemId: command.budgetItemId ?? undefined,
          encumbranceId: command.encumbranceId ?? undefined,
          paymentOrderId: command.paymentOrderId ?? undefined,
          rowVersion: command.rowVersion,
        }),
      ),
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
