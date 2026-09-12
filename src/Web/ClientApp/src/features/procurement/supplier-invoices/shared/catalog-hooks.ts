import { useQuery } from '@tanstack/react-query';
import { api } from '@/shared/api';

interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
}

export function useItems() {
  return useQuery({
    queryKey: ['items'],
    queryFn: async () => {
      const res = await api.get<PaginatedResponse<{ id: number; name: string; code: string }>>('/api/Items', { params: { pageSize: 500 } });
      return res.items ?? [];
    },
    staleTime: Infinity,
  });
}

export function useUnits() {
  return useQuery({
    queryKey: ['units'],
    queryFn: () => api.get<{ id: number; name: string; code: string }[]>('/api/Units'),
    staleTime: Infinity,
  });
}

export function useCurrencies() {
  return useQuery({
    queryKey: ['currencies'],
    queryFn: () => api.get<{ id: number; code: string; name: string }[]>('/api/Currencies'),
    staleTime: Infinity,
  });
}
