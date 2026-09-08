import type {
  RevenueCollectionsReportDto,
  RevenueCollectionsLineDto,
  RevenueCollectionsTotalDto,
  RevenueCollectionsDetailDto,
  RevenueCollectionsLineDetailDto,
  CheckDetailDto,
  FiscalYearDto,
} from '@/web-api-client';

export type {
  RevenueCollectionsReportDto,
  RevenueCollectionsLineDto,
  RevenueCollectionsTotalDto,
  RevenueCollectionsDetailDto,
  RevenueCollectionsLineDetailDto,
  CheckDetailDto,
  FiscalYearDto,
};

export interface RevenueCollectionsFilters {
  fiscalYearId: number;
  fiscalPeriodId?: number;
  fundId?: number;
  revenueAccountId?: number;
  partyId?: number;
  paymentMethod?: string;
}
