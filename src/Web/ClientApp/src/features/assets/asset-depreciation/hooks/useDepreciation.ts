import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { AssetDepreciationSchedule, DepreciationRun, DepreciationRunDetail, PreviewDepreciationRequest, PreviewDepreciationResult, RunDepreciationRequest } from '../shared/types';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const queryKeys = {
  all: ['assetDepreciationRuns'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useDepreciationRuns(params?: { fiscalYearId?: number; status?: string; page?: number }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: async () => {
      const searchParams = new URLSearchParams();
      if (params?.fiscalYearId) searchParams.set('fiscalYearId', String(params.fiscalYearId));
      if (params?.status) searchParams.set('status', params.status);
      if (params?.page) searchParams.set('page', String(params.page));
      return api.get<PaginatedList<DepreciationRun>>(`/api/AssetDepreciation?${searchParams}`);
    },
  });
}

export function useDepreciationRunDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<DepreciationRunDetail>(`/api/AssetDepreciation/${id}`),
    enabled: id > 0,
  });
}

export function useRunDepreciation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: RunDepreciationRequest) => api.post('/api/AssetDepreciation', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function usePreviewDepreciation() {
  return useMutation({
    mutationFn: (data: PreviewDepreciationRequest) => api.post<PreviewDepreciationResult>('/api/AssetDepreciation/preview', data),
  });
}

export function usePostDepreciation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.post(`/api/AssetDepreciation/${id}/post`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useDepreciationSchedules({ assetId }: { assetId: number }) {
  return useQuery({
    queryKey: ['assetDepreciationSchedules', assetId],
    queryFn: () => api.get<PaginatedList<AssetDepreciationSchedule>>(`/api/AssetDepreciation/schedules/asset/${assetId}`),
    enabled: assetId > 0,
  });
}
