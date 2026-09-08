import { useQuery } from '@tanstack/react-query';
import {
  AvailabilitySnapshotReportsClient,
  FiscalYearsClient,
  type AvailabilitySnapshotDto,
  type AvailabilitySnapshotDetailDto,
} from '@/web-api-client';
import { reportingKeys } from '@/shared/api/query-keys';
import type { AvailabilitySnapshotFilters } from '../shared/types';

const reportsClient = new AvailabilitySnapshotReportsClient();
const fiscalYearsClient = new FiscalYearsClient();

export function useAvailabilitySnapshotReport(filters: AvailabilitySnapshotFilters | null) {
  return useQuery({
    queryKey: reportingKeys.availabilitySnapshot(filters),
    queryFn: (): Promise<AvailabilitySnapshotDto> =>
      reportsClient.availabilitySnapshotReports(
        filters!.fiscalYearId,
        filters!.budgetItemId,
        filters!.fundId,
      ),
    enabled: filters !== null,
  });
}

export function useAvailabilitySnapshotDetail(budgetItemId: number | null, fiscalYearId: number) {
  return useQuery({
    queryKey: reportingKeys.availabilitySnapshotDetail(budgetItemId ?? 0),
    queryFn: (): Promise<AvailabilitySnapshotDetailDto> =>
      reportsClient.detail(budgetItemId!, fiscalYearId, undefined),
    enabled: budgetItemId !== null && Number.isFinite(budgetItemId),
  });
}

export function useAvailabilitySnapshotFiscalYears() {
  return useQuery({
    queryKey: ['reporting', 'availability-snapshot', 'fiscal-years'],
    queryFn: () => fiscalYearsClient.fiscalYearsAll(true),
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
