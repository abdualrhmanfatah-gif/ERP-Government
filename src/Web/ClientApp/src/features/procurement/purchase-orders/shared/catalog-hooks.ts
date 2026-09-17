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

export function useSuppliers() {
  return useQuery({
    queryKey: ['suppliers'],
    queryFn: () => api.get<{ id: number; nameAr: string; partyType: string }[]>('/api/Parties'),
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

export function useCurrencies() {
  return useQuery({
    queryKey: ['currencies'],
    queryFn: () => api.get<{ id: number; code: string; nameAr: string; isBase: boolean }[]>('/api/Currencies'),
    staleTime: Infinity,
  });
}

export function useExchangeRateLookup(baseCurrencyId: number, currencyId: number, enabled = true) {
  return useQuery({
    queryKey: ['exchangeRate', baseCurrencyId, currencyId],
    queryFn: () => api.get<{ rate: number; rateDate: string; rateType: string }>(
      '/api/ExchangeRates/lookup',
      { params: { baseCurrencyId, currencyId, date: new Date().toISOString().split('T')[0], rateType: 0 } },
    ),
    enabled: enabled && baseCurrencyId > 0 && currencyId > 0 && baseCurrencyId !== currencyId,
    staleTime: 60_000,
  });
}

export interface PurchaseRequestDetailLine {
  id: number;
  itemId: number;
  unitId: number;
  requestedQuantity: number;
  approvedQuantity?: number;
  unitCostEstimate?: number;
}

export interface PurchaseRequestDetail {
  id: number;
  requestNumber: string;
  status: string;
  lines: PurchaseRequestDetailLine[];
}

export function usePurchaseRequestDetail(id: number) {
  return useQuery({
    queryKey: ['purchaseRequest', id],
    queryFn: () => api.get<PurchaseRequestDetail>(`/api/PurchaseRequests/${id}`),
    enabled: id > 0,
  });
}

export interface QuotationDetailLine {
  id: number;
  purchaseRequestDetailId: number;
  itemId: number;
  unitId: number;
  quantity: number;
  unitPrice?: number;
  discountPercent?: number;
  taxPercent?: number;
}

export interface QuotationDetail {
  id: number;
  quotationNumber: string;
  supplierPartyId: number;
  status: string;
  currencyCode?: string;
  exchangeRate?: number;
  paymentTerms?: string;
  deliveryTerms?: string;
  lines: QuotationDetailLine[];
}

export function useQuotationDetail(id: number) {
  return useQuery({
    queryKey: ['quotation', id],
    queryFn: () => api.get<QuotationDetail>(`/api/Quotations/${id}`),
    enabled: id > 0,
  });
}
