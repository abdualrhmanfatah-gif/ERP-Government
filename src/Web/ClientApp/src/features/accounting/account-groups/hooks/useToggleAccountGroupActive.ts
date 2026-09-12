import { useMutation, useQueryClient } from '@tanstack/react-query';
import { accountGroupsClient } from '../../shared/client';
import type { ToggleAccountGroupActiveCommand } from '../../../../web-api-client';

export function useToggleAccountGroupActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...cmd }: ToggleAccountGroupActiveCommand & { id: number }) =>
      accountGroupsClient.toggleActivePOST(id, cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['account-groups'] });
      qc.invalidateQueries({ queryKey: ['account-group-detail'] });
    },
  });
}
