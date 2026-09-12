import { useQuery } from '@tanstack/react-query';
import { accountGroupsClient } from '../../shared/client';

export function useAccountGroupDetail(id: number) {
  return useQuery({
    queryKey: ['account-group-detail', id],
    queryFn: () => accountGroupsClient.detail5(id),
    enabled: !!id,
  });
}
