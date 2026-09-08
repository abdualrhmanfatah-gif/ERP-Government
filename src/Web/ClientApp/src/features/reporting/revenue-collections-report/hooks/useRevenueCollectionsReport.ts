import { useQuery } from '@tanstack/react-query';
import {
  RevenueCollectionsReportsClient,
  FiscalYearsClient,
  type RevenueCollectionsReportDto,
  type RevenueCollectionsDetailDto,
} from '@/web-api-client';
import { reportingKeys } from '@/shared/api/query-keys';
import type { RevenueCollectionsFilters } from '../shared/types';

const reportsClient = new RevenueCollectionsReportsClient();
const fiscalYearsClient = new FiscalYearsClient();

export function useRevenueCollectionsReport(filters: RevenueCollectionsFilters | null) {
  return useQuery({
    queryKey: reportingKeys.revenueCollections(filters),
    queryFn: (): Promise<RevenueCollectionsReportDto> =>
      reportsClient.revenueCollectionsReports(
        filters!.fiscalYearId,
        filters!.fiscalPeriodId,
        filters!.fundId,
        filters!.revenueAccountId,
        filters!.partyId,
        filters!.paymentMethod,
      ),
    enabled: filters !== null,
  });
}

export function useRevenueCollectionsDetail(receiptVoucherId: number | null) {
  return useQuery({
    queryKey: reportingKeys.revenueCollectionsDetail(receiptVoucherId ?? 0),
    queryFn: (): Promise<RevenueCollectionsDetailDto> =>
      reportsClient.detail4(receiptVoucherId!),
    enabled: receiptVoucherId !== null && Number.isFinite(receiptVoucherId),
  });
}

export function useRevenueCollectionsFiscalYears() {
  return useQuery({
    queryKey: ['reporting', 'revenue-collections', 'fiscal-years'],
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
