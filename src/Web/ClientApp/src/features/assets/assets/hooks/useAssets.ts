import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { Asset, AssetDetail } from '../shared/types';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface GetAssetsParams {
  search?: string;
  status?: string;
  assetGroupId?: number;
  locationId?: number;
  employeeId?: number;
  page?: number;
  pageSize?: number;
}

export const assetKeys = {
  all: ['assets'] as const,
  list: (params: GetAssetsParams) => ['assets', 'list', params] as const,
  detail: (id: number) => ['assets', 'detail', id] as const,
};

export function useAssetsList(params: GetAssetsParams = {}) {
  return useQuery({
    queryKey: assetKeys.list(params),
    queryFn: async () => {
      const searchParams = new URLSearchParams();
      if (params.search) searchParams.set('search', params.search);
      if (params.status) searchParams.set('status', params.status);
      if (params.assetGroupId) searchParams.set('assetGroupId', params.assetGroupId.toString());
      if (params.locationId) searchParams.set('locationId', params.locationId.toString());
      if (params.employeeId) searchParams.set('employeeId', params.employeeId.toString());
      if (params.page) searchParams.set('page', params.page.toString());
      if (params.pageSize) searchParams.set('pageSize', params.pageSize.toString());
      return await api.get<PaginatedList<Asset>>(`/api/Assets?${searchParams.toString()}`);
    },
  });
}

export function useAssetById(id: number) {
  return useQuery({
    queryKey: assetKeys.detail(id),
    queryFn: async () => {
      return await api.get<AssetDetail>(`/api/Assets/${id}`);
    },
    enabled: id > 0,
  });
}

export function useCreateAsset() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (asset: Record<string, unknown>) => {
      return await api.post<number>('/api/Assets', asset);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: assetKeys.all });
    },
  });
}

export function useUpdateAsset() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, data: asset }: { id: number; data: Record<string, unknown> }) => {
      await api.put(`/api/Assets/${id}`, asset);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: assetKeys.all });
      queryClient.invalidateQueries({ queryKey: assetKeys.detail(variables.id) });
    },
  });
}

export function useDeactivateAsset() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, rowVersion }: { id: number; rowVersion: string }) => {
      await api.post(`/api/Assets/${id}/deactivate`, { rowVersion });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: assetKeys.all });
    },
  });
}
