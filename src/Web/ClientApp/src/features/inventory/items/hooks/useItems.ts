import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { Item, ItemDetail, ItemUnit } from '../shared/types';

const queryKeys = {
  all: ['inventoryItems'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
  units: (itemId: number) => [...queryKeys.all, 'units', itemId] as const,
};

export function useItemsList(params?: {
  search?: string;
  categoryId?: number;
  unitId?: number;
  itemType?: string;
  isActive?: boolean;
  underReorderLevel?: boolean;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<{ items: Item[]; totalCount: number }>('/api/Items', { params }),
  });
}

export function useItemById(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<ItemDetail>(`/api/Items/${id}`),
    enabled: id > 0,
  });
}

export function useItemUnits(itemId: number) {
  return useQuery({
    queryKey: queryKeys.units(itemId),
    queryFn: () => api.get<ItemUnit[]>(`/api/Items/${itemId}/units`),
    enabled: itemId > 0,
  });
}

export function useCreateItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: unknown) => api.post<number>('/api/Items', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useUpdateItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: unknown }) =>
      api.put(`/api/Items/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useToggleItemActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/Items/${id}/toggle-active`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useAddItemUnit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ itemId, data }: { itemId: number; data: unknown }) =>
      api.post<number>(`/api/Items/${itemId}/units`, data),
    onSuccess: (_data, { itemId }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.units(itemId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}

export function useUpdateItemUnit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ itemId, id, data }: { itemId: number; id: number; data: unknown }) =>
      api.put(`/api/Items/${itemId}/units/${id}`, data),
    onSuccess: (_data, { itemId }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.units(itemId) });
    },
  });
}

export function useRemoveItemUnit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ itemId, id }: { itemId: number; id: number }) =>
      api.delete(`/api/Items/${itemId}/units/${id}`),
    onSuccess: (_data, { itemId }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.units(itemId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.all });
    },
  });
}
