import { useQuery } from '@tanstack/react-query';
import {
  BudgetExecutionReportsClient,
  FiscalYearsClient,
  FundsClient,
  BudgetClassificationsClient,
  type BudgetExecutionReportDto,
  type BudgetExecutionDetailDto,
} from '@/web-api-client';
import { reportingKeys } from '@/shared/api/query-keys';
import type { BudgetExecutionFilters } from '../shared/types';

const reportsClient = new BudgetExecutionReportsClient();
const fiscalYearsClient = new FiscalYearsClient();
const fundsClient = new FundsClient();
const classificationsClient = new BudgetClassificationsClient();

export function useBudgetExecutionReport(filters: BudgetExecutionFilters | null) {
  return useQuery({
    queryKey: reportingKeys.budgetExecution(filters),
    queryFn: (): Promise<BudgetExecutionReportDto> =>
      reportsClient.budgetExecutionReports(
        filters!.fiscalYearId,
        undefined,
        filters!.fundId,
        filters!.programId,
        filters!.projectId,
        filters!.budgetItemId,
      ),
    enabled: filters !== null,
  });
}

export function useBudgetExecutionDetail(budgetItemId: number | null) {
  return useQuery({
    queryKey: reportingKeys.budgetExecutionDetail(budgetItemId ?? 0),
    queryFn: (): Promise<BudgetExecutionDetailDto> =>
      reportsClient.detail2(budgetItemId!, undefined),
    enabled: budgetItemId !== null && Number.isFinite(budgetItemId),
  });
}

export function useBudgetExecutionFiscalYears() {
  return useQuery({
    queryKey: ['reporting', 'budget-execution', 'fiscal-years'],
    queryFn: () => fiscalYearsClient.fiscalYearsAll(true),
  });
}

export function useBudgetExecutionFilterOptions() {
  const funds = useQuery({
    queryKey: ['reporting', 'budget-execution', 'funds'],
    queryFn: () => fundsClient.fundsAll(),
  });
  const classifications = useQuery({
    queryKey: ['reporting', 'budget-execution', 'classifications'],
    queryFn: () => classificationsClient.budgetClassificationsAll(),
  });
  return { funds, classifications };
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
