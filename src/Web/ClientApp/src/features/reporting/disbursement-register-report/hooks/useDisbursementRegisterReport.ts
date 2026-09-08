import { useQuery } from '@tanstack/react-query';
import {
  DisbursementRegisterReportsClient,
  FiscalYearsClient,
  type DisbursementRegisterDto,
  type DisbursementRegisterDetailDto,
} from '@/web-api-client';
import { reportingKeys } from '@/shared/api/query-keys';
import type { DisbursementRegisterFilters } from '../shared/types';

const reportsClient = new DisbursementRegisterReportsClient();
const fiscalYearsClient = new FiscalYearsClient();

export function useDisbursementRegisterReport(filters: DisbursementRegisterFilters | null) {
  return useQuery({
    queryKey: reportingKeys.disbursementRegister(filters),
    queryFn: (): Promise<DisbursementRegisterDto> =>
      reportsClient.disbursementRegisterReports(
        filters!.fiscalYearId,
        filters!.fiscalPeriodId,
        filters!.fundId,
        filters!.status,
        filters!.approverId,
      ),
    enabled: filters !== null,
  });
}

export function useDisbursementRegisterDetail(paymentOrderId: number | null) {
  return useQuery({
    queryKey: reportingKeys.disbursementRegisterDetail(paymentOrderId ?? 0),
    queryFn: (): Promise<DisbursementRegisterDetailDto> =>
      reportsClient.detail3(paymentOrderId!),
    enabled: paymentOrderId !== null && Number.isFinite(paymentOrderId),
  });
}

export function useDisbursementRegisterFiscalYears() {
  return useQuery({
    queryKey: ['reporting', 'disbursement-register', 'fiscal-years'],
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
