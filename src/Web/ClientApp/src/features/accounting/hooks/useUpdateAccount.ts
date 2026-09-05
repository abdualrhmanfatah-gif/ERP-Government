import { useMutation, useQueryClient } from '@tanstack/react-query';
import { AccountsClient, UpdateAccountCommand } from '../../../web-api-client';

const client = new AccountsClient();

export function useUpdateAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: {
      id?: number; name?: string; description?: string; accountGroupId?: number;
      normalBalance?: number; isPostable?: boolean; isReconcilable?: boolean; rowVersion?: string;
    } }) => {
      const cmd = UpdateAccountCommand.fromJS(data);
      return client.accountsPUT(id, cmd);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['accounts'] });
      qc.invalidateQueries({ queryKey: ['account'] });
    },
  });
}
