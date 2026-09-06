import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { financialSettingsKeys, currenciesClient } from '../shared/client';
import type { CreateCurrencyCommand, UpdateCurrencyCommand, ActivateDeactivateCommand } from '../shared/types';

export function useCurrenciesList(isActive?: boolean) {
  return useQuery({
    queryKey: financialSettingsKeys.currencies.list({ isActive }),
    queryFn: () => currenciesClient.list(isActive),
  });
}

export function useCurrencyDetail(id: number) {
  return useQuery({
    queryKey: financialSettingsKeys.currencies.detail(id),
    queryFn: () => currenciesClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useIso4217Codes(query?: string) {
  return useQuery({
    queryKey: financialSettingsKeys.currencies.iso4217(query),
    queryFn: () => currenciesClient.getIso4217(query),
  });
}

export function useCreateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCurrencyCommand) => currenciesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.currencies.all }),
  });
}

export function useUpdateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: UpdateCurrencyCommand) => currenciesClient.update(id, { id, ...data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.currencies.all }),
  });
}

export function useActivateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: ActivateDeactivateCommand) => currenciesClient.activate(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.currencies.all }),
  });
}

export function useDeactivateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: ActivateDeactivateCommand) => currenciesClient.deactivate(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.currencies.all }),
  });
}
