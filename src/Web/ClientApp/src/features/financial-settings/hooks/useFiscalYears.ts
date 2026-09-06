import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { financialSettingsKeys, fiscalYearsClient } from '../shared/client';
import type { CreateFiscalYearCommand, UpdateFiscalYearCommand } from '../shared/types';

export function useFiscalYearsList(isActive?: boolean) {
  return useQuery({
    queryKey: financialSettingsKeys.fiscalYears.list({ isActive }),
    queryFn: () => fiscalYearsClient.list(isActive),
  });
}

export function useFiscalYearDetail(id: number) {
  return useQuery({
    queryKey: financialSettingsKeys.fiscalYears.detail(id),
    queryFn: () => fiscalYearsClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateFiscalYear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateFiscalYearCommand) => fiscalYearsClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalYears.all }),
  });
}

export function useUpdateFiscalYear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: UpdateFiscalYearCommand) => fiscalYearsClient.update(id, { id, ...data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalYears.all }),
  });
}

export function useOpenFiscalYear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => fiscalYearsClient.open(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalYears.all }),
  });
}

export function useCloseFiscalYear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => fiscalYearsClient.close(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalYears.all }),
  });
}
