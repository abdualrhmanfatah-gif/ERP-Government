import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import type { PurchaseRequest, PurchaseRequestDetail, PaginatedList } from '../shared/types';

const queryKeys = {
  all: ['purchaseRequests'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function usePurchaseRequestsList(params?: {
  status?: string;
  priority?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<PaginatedList<PurchaseRequest>>('/api/PurchaseRequests', { params }),
  });
}

export function usePurchaseRequestDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<PurchaseRequestDetail>(`/api/PurchaseRequests/${id}`),
    enabled: id > 0,
  });
}

function sanitizeDates(data: Record<string, unknown>) {
  const result = { ...data };
  if (result.requiredDate === '' || result.requiredDate === undefined) {
    result.requiredDate = null;
  }
  return result;
}

export function useCreatePurchaseRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: unknown) => api.post<number>('/api/PurchaseRequests', sanitizeDates(data as Record<string, unknown>)),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useUpdatePurchaseRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: unknown }) => {
      const payload = { ...sanitizeDates(data as Record<string, unknown>), id };
      return api.put(`/api/PurchaseRequests/${id}`, payload);
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useSubmitPurchaseRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/PurchaseRequests/${id}/submit`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useApprovePurchaseRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/PurchaseRequests/${id}/approve`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useRejectPurchaseRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: number; reason: string }) =>
      api.patch(`/api/PurchaseRequests/${id}/reject`, { reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useCancelPurchaseRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      api.patch(`/api/PurchaseRequests/${id}/cancel`, { reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}
