import { useQuery } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { ComboboxOption } from '@/components/ui/Combobox';

interface CatalogItem {
  id: number;
  code: string;
  name: string;
}

interface PaginatedItems {
  items: CatalogItem[];
  totalCount: number;
}

const catalogKeys = {
  items: ['catalog', 'items'] as const,
  units: ['catalog', 'units'] as const,
  departments: ['catalog', 'departments'] as const,
  costCenters: ['catalog', 'costCenters'] as const,
};

export function useItemsList() {
  return useQuery({
    queryKey: catalogKeys.items,
    queryFn: () => api.get<PaginatedItems>('/api/Items', { params: { pageSize: 500 } }),
    staleTime: Infinity,
    select: (data): ComboboxOption[] =>
      data.items.map((item) => ({
        value: String(item.id),
        label: `${item.code} - ${item.name}`,
      })),
  });
}

export function useUnitsList() {
  return useQuery({
    queryKey: catalogKeys.units,
    queryFn: () => api.get<CatalogItem[]>('/api/Units'),
    staleTime: Infinity,
    select: (data): ComboboxOption[] =>
      data.map((unit) => ({
        value: String(unit.id),
        label: `${unit.code} - ${unit.name}`,
      })),
  });
}

export function useDepartmentsList() {
  return useQuery({
    queryKey: catalogKeys.departments,
    queryFn: () => api.get<CatalogItem[]>('/api/OrganizationalUnits'),
    staleTime: Infinity,
    select: (data): ComboboxOption[] =>
      data.map((dept) => ({
        value: String(dept.id),
        label: `${dept.code} - ${dept.name}`,
      })),
  });
}

export function useCostCentersList() {
  return useQuery({
    queryKey: catalogKeys.costCenters,
    queryFn: () => api.get<CatalogItem[]>('/api/CostCenters'),
    staleTime: Infinity,
    select: (data): ComboboxOption[] =>
      data.map((cc) => ({
        value: String(cc.id),
        label: `${cc.code} - ${cc.name}`,
      })),
  });
}
