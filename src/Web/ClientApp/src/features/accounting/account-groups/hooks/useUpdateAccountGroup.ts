import { useMutation, useQueryClient } from '@tanstack/react-query';
import { accountGroupsClient } from '../../shared/client';
import type { UpdateAccountGroupCommand } from '../../../../web-api-client';

export function useUpdateAccountGroup() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateAccountGroupCommand & { id: number }) =>
      accountGroupsClient.accountGroupsPUT(payload.id, payload),
    onSuccess: (_, vars) => {
      qc.invalidateQueries({ queryKey: ['account-groups'] });
      qc.invalidateQueries({ queryKey: ['account-group-detail', vars.id] });
    },
  });
}
