import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';

export interface Warehouse {
  id: number;
  code: string;
  name: string;
  locationId?: number;
  locationName?: string;
  managerId?: number;
  managerName?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  totalCapacity?: number;
  currentLoad?: number;
  isActive: boolean;
}

const queryKeys = {
  all: ['inventoryWarehouses'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useWarehousesList(params?: { search?: string; isActive?: boolean }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<Warehouse[]>('/api/Warehouses', { params }),
  });
}

export function useWarehouseById(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<Warehouse>(`/api/Warehouses/${id}`),
    enabled: id > 0,
  });
}

export function useCreateWarehouse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: unknown) => api.post<number>('/api/Warehouses', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useUpdateWarehouse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: unknown }) =>
      api.put(`/api/Warehouses/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useToggleWarehouseActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.patch(`/api/Warehouses/${id}/toggle-active`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}
