import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import type { GRN, GRNDetail, PaginatedList } from '../shared/types';
import type { CreateGRNFormData } from '../shared/schemas';

const queryKeys = {
  all: ['grns'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useGRNsList(params?: {
  status?: string;
  search?: string;
  purchaseOrderId?: number;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<PaginatedList<GRN>>('/api/GoodsReceiptNotes', { params }),
  });
}

export function useGRNDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<GRNDetail>(`/api/GoodsReceiptNotes/${id}`),
    enabled: id > 0,
  });
}

export function useCreateGRN() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateGRNFormData) => api.post<number>('/api/GoodsReceiptNotes', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useConfirmGRN() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/GoodsReceiptNotes/${id}/confirm`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useRejectGRN() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notes }: { id: number; notes: string }) =>
      api.patch(`/api/GoodsReceiptNotes/${id}/reject`, { notes }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}
