import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { Quotation, QuotationDetail, PaginatedList } from '../shared/types';

const queryKeys = {
  all: ['quotations'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useQuotationsList(params?: {
  supplierPartyId?: number;
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<PaginatedList<Quotation>>('/api/Quotations', { params }),
  });
}

export function useQuotationDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<QuotationDetail>(`/api/Quotations/${id}`),
    enabled: id > 0,
  });
}

export function useCreateQuotation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: unknown) => api.post<number>('/api/Quotations', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useUpdateQuotation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: unknown }) =>
      api.put(`/api/Quotations/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useSubmitQuotation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/Quotations/${id}/submit`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useStartEvaluation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/Quotations/${id}/start-evaluation`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useCompleteEvaluation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, technicalScore, financialScore }: { id: number; technicalScore: number; financialScore: number }) =>
      api.patch(`/api/Quotations/${id}/complete-evaluation`, { technicalScore, financialScore }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useSelectQuotation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, selectionReason }: { id: number; selectionReason: string }) =>
      api.patch(`/api/Quotations/${id}/select`, { selectionReason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useAwardQuotation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/Quotations/${id}/award`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useRejectQuotation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rejectionReason }: { id: number; rejectionReason: string }) =>
      api.patch(`/api/Quotations/${id}/reject`, { rejectionReason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}
