import { useMutation, useQueryClient } from '@tanstack/react-query';
import { AccountsClient, CreateAccountCommand } from '../../../web-api-client';

const client = new AccountsClient();

export function useCreateAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) => {
      const cmd = CreateAccountCommand.fromJS(data);
      return client.accountsPOST(cmd);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['accounts'] }),
  });
}
