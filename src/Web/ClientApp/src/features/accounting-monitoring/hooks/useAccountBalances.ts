import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { accountingBalancesClient } from '../shared/client';

export function useAccountBalances(filters: {
  fiscalYearId: number;
  fiscalPeriodId: number;
  accountId?: number;
  currencyId?: number;
}) {
  return useQuery({
    queryKey: ['accountBalances', filters],
    queryFn: () =>
      accountingBalancesClient.accountingBalances(
        filters.fiscalYearId,
        filters.fiscalPeriodId,
        filters.accountId,
        filters.currencyId,
      ),
    enabled: !!filters.fiscalYearId && !!filters.fiscalPeriodId,
  });
}

export function useFinalizePeriod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { fiscalYearId: number; fiscalPeriodId: number }) =>
      accountingBalancesClient.finalize(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accountBalances'] });
    },
  });
}

export function useUnfinalizePeriod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { fiscalYearId: number; fiscalPeriodId: number }) =>
      accountingBalancesClient.unfinalize(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accountBalances'] });
    },
  });
}

export function useRebuildBalances() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { fiscalYearId: number; fiscalPeriodId?: number }) =>
      accountingBalancesClient.rebuild(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accountBalances'] });
    },
  });
}
