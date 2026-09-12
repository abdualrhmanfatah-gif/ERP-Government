import { useQuery } from '@tanstack/react-query';
import {
  ReportsClient,
  TrialBalanceReportsClient,
  FiscalYearsClient,
  FiscalPeriodsClient,
  type BalanceSheetDto,
  type IncomeStatementDto,
  type CashFlowStatementDto,
  type GeneralLedgerDto,
  type TrialBalanceReportDto,
} from '@/web-api-client';
import { reportingKeys } from '@/shared/api/query-keys';
import type { FinancialStatementFilters, GeneralLedgerFilters } from '../shared/types';

const reportsClient = new ReportsClient();
const trialBalanceClient = new TrialBalanceReportsClient();
const fiscalYearsClient = new FiscalYearsClient();
const fiscalPeriodsClient = new FiscalPeriodsClient();

export function useBalanceSheet(filters: FinancialStatementFilters | null) {
  return useQuery({
    queryKey: reportingKeys.balanceSheet(filters),
    queryFn: (): Promise<BalanceSheetDto> =>
      reportsClient.balanceSheet(filters!.asOfDate, filters!.fiscalPeriodId),
    enabled: filters !== null,
  });
}

export function useIncomeStatement(filters: FinancialStatementFilters | null) {
  return useQuery({
    queryKey: reportingKeys.incomeStatement(filters),
    queryFn: (): Promise<IncomeStatementDto> =>
      reportsClient.incomeStatement(filters!.startDate, filters!.endDate),
    enabled: filters !== null,
  });
}

export function useCashFlowStatement(filters: FinancialStatementFilters | null) {
  return useQuery({
    queryKey: reportingKeys.cashFlowStatement(filters),
    queryFn: (): Promise<CashFlowStatementDto> =>
      reportsClient.cashFlow(filters!.startDate, filters!.endDate),
    enabled: filters !== null,
  });
}

export function useGeneralLedger(filters: GeneralLedgerFilters | null) {
  return useQuery({
    queryKey: reportingKeys.generalLedger(filters),
    queryFn: (): Promise<GeneralLedgerDto> =>
      reportsClient.generalLedger(
        filters!.accountId,
        filters!.accountCode,
        filters!.fiscalPeriodId,
        filters!.startDate,
        filters!.endDate,
        filters!.page,
        filters!.pageSize,
      ),
    enabled: filters !== null,
  });
}

export function useTrialBalanceLegacy(filters: { fiscalYearId: number; fiscalPeriodId?: number } | null) {
  return useQuery({
    queryKey: reportingKeys.trialBalanceLegacy(filters),
    queryFn: (): Promise<TrialBalanceReportDto> =>
      trialBalanceClient.trialBalanceReports(filters!.fiscalYearId, filters!.fiscalPeriodId),
    enabled: filters !== null,
  });
}

export function useFinancialStatementFiscalYears() {
  return useQuery({
    queryKey: ['reporting', 'financial-statements', 'fiscal-years'],
    queryFn: () => fiscalYearsClient.fiscalYearsAll(true),
  });
}

export function useFinancialStatementPeriods(fiscalYearId: number | undefined) {
  return useQuery({
    queryKey: ['reporting', 'financial-statements', 'periods', fiscalYearId],
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
