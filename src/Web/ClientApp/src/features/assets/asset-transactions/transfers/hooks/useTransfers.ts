import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { TransferDetail, TransferListItem } from '../shared/types';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const queryKeys = {
  all: ['assetTransfers'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useTransfersList(params?: { search?: string; status?: string; page?: number }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: async () => {
      const searchParams = new URLSearchParams();
      if (params?.search) searchParams.set('search', params.search);
      if (params?.status) searchParams.set('status', params.status);
      if (params?.page) searchParams.set('page', params.page.toString());
      return await api.get<PaginatedList<TransferListItem>>(`/api/AssetTransfers?${searchParams.toString()}`);
    },
  });
}

export function useTransferDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<TransferDetail>(`/api/AssetTransfers/${id}`),
    enabled: id > 0,
  });
}

export function useCreateTransfer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) => api.post<number>('/api/AssetTransfers', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}

export function useUpdateTransfer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: Record<string, unknown> }) =>
      api.put<number>(`/api/AssetTransfers/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}

export function useExecuteTransfer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion, assetRowVersion }: { id: number; rowVersion: string; assetRowVersion: string }) =>
      api.post<number>(`/api/AssetTransfers/${id}/execute`, { rowVersion, assetRowVersion }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
      queryClient.invalidateQueries({ queryKey: ['assets'] });
    },
  });
}

export function useCancelTransfer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      api.post<number>(`/api/AssetTransfers/${id}/cancel`, { rowVersion }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}
