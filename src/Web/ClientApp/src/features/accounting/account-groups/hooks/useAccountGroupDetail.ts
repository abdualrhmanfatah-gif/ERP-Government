import { useQuery } from '@tanstack/react-query';
import type { AccountGroupDetailResponse } from '../types';

export function useAccountGroupDetail(id: number) {
  return useQuery({
    queryKey: ['account-group-detail', id],
    queryFn: async (): Promise<AccountGroupDetailResponse> => {
      const res = await fetch(`/api/AccountGroups/${id}/detail`, { credentials: 'include' });
      if (!res.ok) throw new Error(await res.text());
      return res.json();
    },
    enabled: !!id,
  });
}
