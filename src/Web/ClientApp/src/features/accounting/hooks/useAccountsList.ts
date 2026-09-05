import { useQuery } from '@tanstack/react-query';
import { AccountsClient } from '../../../web-api-client';

const client = new AccountsClient();

export function useAccountsList(filters?: {
  isActive?: boolean;
  accountGroupId?: number;
  isPostable?: boolean;
  normalBalance?: number;
}) {
  return useQuery({
    queryKey: ['accounts', filters],
    queryFn: () =>
      client.accountsAll(
        filters?.isActive,
        filters?.accountGroupId,
        filters?.isPostable,
        filters?.normalBalance,
      ),
  });
}
