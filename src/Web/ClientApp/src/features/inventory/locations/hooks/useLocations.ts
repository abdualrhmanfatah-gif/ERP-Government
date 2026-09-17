import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';

export interface Location {
  id: number;
  code: string;
  name: string;
  barcode?: string;
  parentLocationId?: number;
  parentName?: string;
  level?: number;
  breadcrumb?: string;
  city?: string;
  address?: string;
  capacity?: number;
  isActive: boolean;
  rowVersion: string;
}

const queryKeys = {
  all: ['inventoryLocations'] as const,
  list: (params?: Record<string, unknown>) => [...queryKeys.all, 'list', params] as const,
  detail: (id: number) => [...queryKeys.all, 'detail', id] as const,
};

export function useLocationsList(params?: { search?: string; isActive?: boolean; parentLocationId?: number }) {
  return useQuery({
    queryKey: queryKeys.list(params),
    queryFn: () => api.get<Location[]>('/api/Locations', { params }),
  });
}

export function useActiveLocations() {
  return useLocationsList({ isActive: true });
}

export function useLocationById(id: number) {
  return useQuery({
    queryKey: queryKeys.detail(id),
    queryFn: () => api.get<Location>(`/api/Locations/${id}`),
    enabled: id > 0,
  });
}

export function useCreateLocation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: unknown) => api.post<number>('/api/Locations', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useUpdateLocation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: unknown }) =>
      api.put(`/api/Locations/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}

export function useToggleLocationActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, activate, rowVersion }: { id: number; activate: boolean; rowVersion: string }) =>
      api.patch<number>(`/api/Locations/${id}/${activate ? 'activate' : 'deactivate'}`, { rowVersion }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.all }),
  });
}
