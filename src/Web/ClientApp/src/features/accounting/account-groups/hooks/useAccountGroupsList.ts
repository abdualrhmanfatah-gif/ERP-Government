import { useQuery } from '@tanstack/react-query';
import { accountGroupsClient } from '../../shared/client';
import type { AccountGroupType } from '../../../../web-api-client';

type Params = {
  search?: string;
  type?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
};

export function useAccountGroupsList(params: Params) {
  return useQuery({
    queryKey: ['account-groups', params],
    queryFn: () => accountGroupsClient.accountGroupsGET(
      params.search,
      params.isActive,
      params.type as AccountGroupType | null | undefined,
      params.page ?? 1,
      params.pageSize ?? 20,
    ),
    staleTime: 30_000,
  });
}
