import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { AssetGroup, AssetGroupDetail, CreateAssetGroupRequest, UpdateAssetGroupRequest } from '../shared/types';

const queryKeys = {
  all: ['assetGroups'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useAssetGroupsList(params?: { search?: string; isActive?: boolean; parentId?: number }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<AssetGroup[]>('/api/AssetGroups', { params }),
  });
}

export function useAssetGroupDetail(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<AssetGroupDetail>(`/api/AssetGroups/${id}/detail`),
    enabled: id > 0,
  });
}

export function useCreateAssetGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateAssetGroupRequest) =>
      api.post<number>('/api/AssetGroups', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}

export function useUpdateAssetGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateAssetGroupRequest }) =>
      api.put<number>(`/api/AssetGroups/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}

export function useToggleAssetGroupActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, activate, rowVersion }: { id: number; activate: boolean; rowVersion: string }) =>
      api.post<number>(`/api/AssetGroups/${id}/${activate ? 'activate' : 'deactivate'}`, { rowVersion }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}

export function useSaveGroupAttributeBindings() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ groupId, bindings }: { groupId: number; bindings: { assetAttributeDefinitionId: number; isRequired: boolean; sortOrder: number | null }[] }) =>
      api.put<number>(`/api/AssetAttributes/groups/${groupId}/attributes`, { assetGroupId: groupId, bindings }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}
