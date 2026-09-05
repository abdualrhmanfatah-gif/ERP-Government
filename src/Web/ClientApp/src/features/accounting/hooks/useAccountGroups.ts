import { useQuery } from '@tanstack/react-query';
import { AccountGroupsClient } from '../../../web-api-client';

const client = new AccountGroupsClient();

export function useAccountGroups() {
  return useQuery({
    queryKey: ['accountGroups'],
    queryFn: () => client.accountGroupsAll(undefined, undefined),
  });
}
