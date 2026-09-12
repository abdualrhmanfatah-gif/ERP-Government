import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { SupplierInvoice, SupplierInvoiceDetail, PaginatedList } from '../shared/types';
import type { CreateSupplierInvoiceFormData } from '../shared/schemas';
import { handleLifecycleError } from '@/shared/api/result-to-ui';

const queryKeys = {
  all: ['supplierInvoices'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useSupplierInvoicesList(params?: {
  status?: string;
  search?: string;
  purchaseOrderId?: number;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<PaginatedList<SupplierInvoice>>('/api/SupplierInvoices', { params }),
  });
}

export function useSupplierInvoiceDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<SupplierInvoiceDetail>(`/api/SupplierInvoices/${id}`),
    enabled: id > 0,
  });
}

export function useCreateSupplierInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateSupplierInvoiceFormData) => api.post<number>('/api/SupplierInvoices', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useSubmitSupplierInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/SupplierInvoices/${id}/submit`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useMatchSupplierInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/SupplierInvoices/${id}/match`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}

export function useCancelSupplierInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notes }: { id: number; notes?: string }) =>
      api.patch(`/api/SupplierInvoices/${id}/cancel`, notes ? { notes } : undefined),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
    onError: handleLifecycleError,
  });
}
