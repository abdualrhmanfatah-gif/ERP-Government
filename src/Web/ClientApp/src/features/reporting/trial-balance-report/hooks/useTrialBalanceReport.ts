import { useQuery } from '@tanstack/react-query';
import {
  TrialBalanceReportsClient,
  FiscalYearsClient,
  FiscalPeriodsClient,
  type TrialBalanceReportDto,
  type LedgerMovementDto,
} from '@/web-api-client';
import { reportingKeys } from '@/shared/api/query-keys';
import type { TrialBalanceFilters } from '../shared/types';

const reportsClient = new TrialBalanceReportsClient();
const fiscalYearsClient = new FiscalYearsClient();
const fiscalPeriodsClient = new FiscalPeriodsClient();

export function useTrialBalanceReport(filters: TrialBalanceFilters | null) {
  return useQuery({
    queryKey: reportingKeys.trialBalance(filters),
    queryFn: (): Promise<TrialBalanceReportDto> =>
      reportsClient.trialBalanceReports(
        filters!.fiscalYearId,
        filters!.fiscalPeriodId,
      ),
    enabled: filters !== null,
  });
}

export function useLedgerMovement(accountId: number | null, fiscalYearId: number) {
  return useQuery({
    queryKey: reportingKeys.ledgerMovement(accountId ?? 0, fiscalYearId),
    queryFn: (): Promise<LedgerMovementDto> =>
      reportsClient.ledgerMovement(accountId!, fiscalYearId),
    enabled: accountId !== null && Number.isFinite(accountId),
  });
}

export function useTrialBalanceFiscalYears() {
  return useQuery({
    queryKey: ['reporting', 'trial-balance', 'fiscal-years'],
    queryFn: () => fiscalYearsClient.fiscalYearsAll(true),
  });
}

export function useTrialBalancePeriods(fiscalYearId: number | undefined) {
  return useQuery({
    queryKey: ['reporting', 'trial-balance', 'periods', fiscalYearId],
    queryFn: () => fiscalPeriodsClient.fiscalPeriodsAll(fiscalYearId!),
    enabled: fiscalYearId !== undefined,
  });
}

export function selectDefaultFiscalYear<T extends { status?: string; isActive?: boolean; yearNumber?: number }>(
  years: T[] | undefined,
): T | undefined {
  if (!years || years.length === 0) return undefined;
  return (
    years.find((y) => y.status === 'Open') ??
    years.find((y) => y.isActive) ??
    [...years].sort((a, b) => (b.yearNumber ?? 0) - (a.yearNumber ?? 0))[0]
  );
}
