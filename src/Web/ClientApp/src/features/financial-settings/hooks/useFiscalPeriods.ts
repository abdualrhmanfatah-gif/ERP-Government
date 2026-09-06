import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { financialSettingsKeys, fiscalPeriodsClient } from '../shared/client';
import type { BulkGeneratePeriodsCommand } from '../shared/types';

export function useFiscalPeriodsList(fiscalYearId: number) {
  return useQuery({
    queryKey: financialSettingsKeys.fiscalPeriods.list(fiscalYearId),
    queryFn: () => fiscalPeriodsClient.list(fiscalYearId),
    enabled: Number.isFinite(fiscalYearId),
  });
}

export function useFiscalPeriodDetail(id: number) {
  return useQuery({
    queryKey: financialSettingsKeys.fiscalPeriods.detail(id),
    queryFn: () => fiscalPeriodsClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useBulkGeneratePeriods() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: BulkGeneratePeriodsCommand) => fiscalPeriodsClient.bulkGenerate(data),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalPeriods.all });
      qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalYears.detail(variables.fiscalYearId) });
    },
  });
}

export function useLockFiscalPeriod() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => fiscalPeriodsClient.lock(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalPeriods.all }),
  });
}

export function useUnlockFiscalPeriod() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => fiscalPeriodsClient.unlock(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalPeriods.all }),
  });
}
