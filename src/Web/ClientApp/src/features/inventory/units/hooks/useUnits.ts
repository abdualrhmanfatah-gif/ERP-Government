import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';

export interface Unit {
  id: number;
  code: string;
  name: string;
  nameAr?: string;
  unitType?: string;
  baseUnitId?: number;
  baseUnitName?: string;
  conversionToBase?: number;
  isActive: boolean;
}

const queryKeys = {
  all: ['inventoryUnits'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useUnitsList(params?: { search?: string; isActive?: boolean }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<Unit[]>('/api/Units', { params }),
  });
}

export function useUnitById(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<Unit>(`/api/Units/${id}`),
    enabled: id > 0,
  });
}

export function useCreateUnit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: unknown) => api.post<number>('/api/Units', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useUpdateUnit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: unknown }) =>
      api.put(`/api/Units/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useToggleUnitActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/Units/${id}/toggle-active`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}
