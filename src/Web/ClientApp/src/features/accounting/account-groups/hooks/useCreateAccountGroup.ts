import { useMutation, useQueryClient } from '@tanstack/react-query';
import { accountGroupsClient } from '../../shared/client';
import type { CreateAccountGroupCommand } from '../../../../web-api-client';

export function useCreateAccountGroup() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateAccountGroupCommand) => accountGroupsClient.accountGroupsPOST(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['account-groups'] }),
  });
}
