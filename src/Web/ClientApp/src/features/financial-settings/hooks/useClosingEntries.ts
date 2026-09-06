import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { financialSettingsKeys, closingEntriesClient } from '../shared/client';
import type { GenerateYearEndClosingCommand, ReverseClosingEntryCommand } from '../shared/types';

export function useClosingEntriesByFiscalYear(fiscalYearId: number) {
  return useQuery({
    queryKey: financialSettingsKeys.closingEntries.list(fiscalYearId),
    queryFn: () => closingEntriesClient.listByFiscalYear(fiscalYearId),
    enabled: Number.isFinite(fiscalYearId),
  });
}

export function useClosingEntryDetail(id: number) {
  return useQuery({
    queryKey: financialSettingsKeys.closingEntries.detail(id),
    queryFn: () => closingEntriesClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useGenerateClosingEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: GenerateYearEndClosingCommand) => closingEntriesClient.generate(data),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: financialSettingsKeys.closingEntries.all });
      qc.invalidateQueries({ queryKey: financialSettingsKeys.fiscalYears.detail(variables.fiscalYearId) });
    },
  });
}

export function useApproveClosingEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => closingEntriesClient.approve(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.closingEntries.all }),
  });
}

export function useReverseClosingEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: ReverseClosingEntryCommand) => closingEntriesClient.reverse(id, { id, ...data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: financialSettingsKeys.closingEntries.all }),
  });
}
