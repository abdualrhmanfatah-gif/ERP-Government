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

export function useWarehouses() {
  return useQuery({
    queryKey: ['warehouses'],
    queryFn: () => api.get<{ id: number; name: string; code: string }[]>('/api/Warehouses'),
    staleTime: Infinity,
  });
}

export function useLocations() {
  return useQuery({
    queryKey: ['locations'],
    queryFn: () => api.get<{ id: number; name: string }[]>('/api/Locations'),
    staleTime: Infinity,
  });
}

export interface PurchaseOrderDetailLine {
  id: number;
  itemId: number;
  unitId: number;
  orderedQuantity: number;
  receivedQuantity: number;
  remainingQuantity: number;
}

export interface PurchaseOrderForGRN {
  id: number;
  poNumber: string;
  supplierPartyId: number;
  supplierName?: string;
  status: string;
  lines: PurchaseOrderDetailLine[];
}

export function usePurchaseOrderForGRN(id: number) {
  return useQuery({
    queryKey: ['purchaseOrder', id],
    queryFn: () => api.get<PurchaseOrderForGRN>(`/api/PurchaseOrders/${id}`),
    enabled: id > 0,
  });
}
