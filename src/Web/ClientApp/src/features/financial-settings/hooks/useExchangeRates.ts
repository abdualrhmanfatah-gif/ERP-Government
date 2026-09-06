import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { financialSettingsKeys, exchangeRatesClient } from '../shared/client';
import type { CreateExchangeRateCommand, UpdateExchangeRateCommand, ActivateDeactivateCommand } from '../shared/types';

export function useExchangeRatesList(filters?: {
  currencyId?: number;
  rateType?: number;
  fromDate?: string;
  toDate?: string;
  isActive?: boolean;
}) {
  return useQuery({
    queryKey: financialSettingsKeys.exchangeRates.list(filters),
    queryFn: () => exchangeRatesClient.list(filters),
  });
}

export function useExchangeRateDetail(id: number) {
  return useQuery({
    queryKey: financialSettingsKeys.exchangeRates.detail(id),
    queryFn: () => exchangeRatesClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useLookupExchangeRate(params: {
  baseCurrencyId: number;
  currencyId: number;
  date: string;
} | null) {
  return useQuery({
    queryKey: financialSettingsKeys.exchangeRates.lookup(params ?? {}),
    queryFn: () => exchangeRatesClient.lookup(params!),
    enabled: !!params && Number.isFinite(params.baseCurrencyId) && Number.isFinite(params.currencyId),
  });
}

export function useCreateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateExchangeRateCommand) => exchangeRatesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.exchangeRates.all }),
  });
}

export function useUpdateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: UpdateExchangeRateCommand) =>
      exchangeRatesClient.update(id, { id, ...data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.exchangeRates.all }),
  });
}

export function useActivateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: ActivateDeactivateCommand) => exchangeRatesClient.activate(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.exchangeRates.all }),
  });
}

export function useDeactivateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: ActivateDeactivateCommand) => exchangeRatesClient.deactivate(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.exchangeRates.all }),
  });
}
