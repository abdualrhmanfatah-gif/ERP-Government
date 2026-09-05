import { useQuery } from '@tanstack/react-query';
import type { PaginatedAccountGroupsResponse } from '../types';

type Params = {
  search?: string;
  type?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
};

async function fetchList(params: Params): Promise<PaginatedAccountGroupsResponse> {
  const qs = new URLSearchParams();
  if (params.search) qs.set('search', params.search);
  if (params.type) qs.set('type', params.type);
  if (params.isActive !== undefined) qs.set('isActive', String(params.isActive));
  qs.set('page', String(params.page ?? 1));
  qs.set('pageSize', String(params.pageSize ?? 20));
  const res = await fetch(`/api/AccountGroups?${qs.toString()}`, { credentials: 'include' });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}

export function useAccountGroupsList(params: Params) {
  return useQuery({
    queryKey: ['account-groups', params],
    queryFn: () => fetchList(params),
    staleTime: 30_000,
  });
}
