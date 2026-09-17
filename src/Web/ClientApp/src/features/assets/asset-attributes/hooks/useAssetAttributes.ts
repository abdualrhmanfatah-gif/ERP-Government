import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type {
  AssetAttributeDefinition,
  PaginatedList,
  CreateAssetAttributeDefinitionRequest,
  UpdateAssetAttributeDefinitionRequest,
} from '../shared/types';

const queryKeys = {
  all: ['assetAttributes'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useAssetAttributesList(params?: {
  search?: string;
  dataType?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<PaginatedList<AssetAttributeDefinition>>('/api/AssetAttributes/definitions', { params }),
  });
}

export function useAssetAttributeDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<AssetAttributeDefinition>(`/api/AssetAttributes/definitions/${id}`),
    enabled: id > 0,
  });
}

export function useCreateAssetAttribute() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateAssetAttributeDefinitionRequest) =>
      api.post<number>('/api/AssetAttributes/definitions', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}

export function useUpdateAssetAttribute() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateAssetAttributeDefinitionRequest }) =>
      api.put<number>(`/api/AssetAttributes/definitions/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}
