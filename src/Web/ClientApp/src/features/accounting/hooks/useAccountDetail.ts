import { useQuery } from '@tanstack/react-query';
import { AccountsClient } from '../../../web-api-client';

const client = new AccountsClient();

export function useAccountDetail(id: number | null) {
  return useQuery({
    queryKey: ['account', id],
    queryFn: () => client.accountsGET(id!),
    enabled: id !== null && id > 0,
  });
}
