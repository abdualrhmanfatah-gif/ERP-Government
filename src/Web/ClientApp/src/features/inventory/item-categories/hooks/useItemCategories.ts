import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';

export interface ItemCategory {
  id: number;
  code: string;
  name: string;
  nameEn?: string;
  description?: string;
  parentItemCategoryId?: number;
  parentName?: string;
  level?: number;
  breadcrumb?: string;
  isActive: boolean;
}

const queryKeys = {
  all: ['inventoryItemCategories'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useItemCategoriesList(params?: { search?: string; isActive?: boolean }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<ItemCategory[]>('/api/ItemCategories', { params }),
  });
}

export function useItemCategoryById(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<ItemCategory>(`/api/ItemCategories/${id}`),
    enabled: id > 0,
  });
}

export function useCreateItemCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: unknown) => api.post<number>('/api/ItemCategories', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useUpdateItemCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: unknown }) =>
      api.put(`/api/ItemCategories/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useToggleItemCategoryActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/ItemCategories/${id}/toggle-active`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}
