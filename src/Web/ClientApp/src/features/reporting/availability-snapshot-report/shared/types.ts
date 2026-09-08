import type {
  AvailabilitySnapshotDto,
  AvailabilitySnapshotLineDto,
  AvailabilitySnapshotDetailDto,
  AppropriationDetailDto,
  EncumbranceDetailDto,
  PaymentDetailDto,
  FiscalYearDto,
} from '@/web-api-client';

export type {
  AvailabilitySnapshotDto,
  AvailabilitySnapshotLineDto,
  AvailabilitySnapshotDetailDto,
  AppropriationDetailDto,
  EncumbranceDetailDto,
  PaymentDetailDto,
  FiscalYearDto,
};

export interface AvailabilitySnapshotFilters {
  fiscalYearId: number;
  budgetItemId?: number;
  fundId?: number;
}
