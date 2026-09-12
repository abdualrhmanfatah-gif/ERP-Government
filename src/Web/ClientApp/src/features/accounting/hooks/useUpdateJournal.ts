import { useMutation, useQueryClient } from '@tanstack/react-query';
import { JournalsClient, UpdateJournalCommand } from '../../../web-api-client';

const client = new JournalsClient();

export function useUpdateJournal() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: {
      id: number;
      data: {
        id?: number;
        name?: string;
        type?: string;
        accountId?: number;
        suspenseAccountId?: number;
        allowForeignCurrency?: boolean;
        requireApprovalBeforePosting?: boolean;
        rowVersion?: string;
      };
    }) => {
      const cmd = UpdateJournalCommand.fromJS(data);
      return client.journalsPUT(id, cmd);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['journals'] });
      qc.invalidateQueries({ queryKey: ['journal'] });
    },
  });
}
