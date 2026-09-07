import { useMutation, useQueryClient } from '@tanstack/react-query';
import { JournalsClient, CreateJournalCommand } from '../../../web-api-client';

const client = new JournalsClient();

export function useCreateJournal() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) => {
      const cmd = CreateJournalCommand.fromJS(data);
      return client.journalsPOST(cmd);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['journals'] }),
  });
}
