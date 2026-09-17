import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { Disposal, DisposalDetail } from '../shared/types';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const queryKeys = {
  all: ['assetDisposals'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useDisposalsList(params?: { search?: string; status?: string; page?: number }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: async () => {
      const sp = new URLSearchParams();
      if (params?.search) sp.set('search', params.search);
      if (params?.status) sp.set('status', params.status);
      if (params?.page) sp.set('page', params.page.toString());
      return await api.get<PaginatedList<Disposal>>(`/api/AssetDisposals?${sp.toString()}`);
    },
  });
}

export function useDisposalDetail(id: number) {
  return useQuery({ queryKey: queryKeys.detail(id), queryFn: () => api.get<DisposalDetail>(`/api/AssetDisposals/${id}`), enabled: id > 0 });
}

export function useCreateDisposal() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: (d: Record<string, unknown>) => api.post<number>('/api/AssetDisposals', d), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) });
}

export function useApproveDisposal() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) => api.post(`/api/AssetDisposals/${id}/approve`, { rowVersion }), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) });
}

export function usePostDisposal() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) => api.post(`/api/AssetDisposals/${id}/post`, { rowVersion }), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) });
}
