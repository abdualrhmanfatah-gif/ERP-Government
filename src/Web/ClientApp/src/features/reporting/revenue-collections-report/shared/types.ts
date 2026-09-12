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

export const paymentMethodLabels: Record<string, string> = {
  Cash: 'نقدي',
  Check: 'شيك',
  Transfer: 'تحويل',
};

export const depositStatusLabels: Record<string, string> = {
  Pending: 'قيد الانتظار',
  Deposited: 'تم الإيداع',
  Rejected: 'مرفوض',
};
