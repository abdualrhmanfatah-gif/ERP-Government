import type {
  DisbursementRegisterDto,
  DisbursementRegisterLineDto,
  DisbursementRegisterTotalDto,
  DisbursementRegisterDetailDto,
  PaymentDetailDto,
  FiscalYearDto,
} from '@/web-api-client';

export type {
  DisbursementRegisterDto,
  DisbursementRegisterLineDto,
  DisbursementRegisterTotalDto,
  DisbursementRegisterDetailDto,
  PaymentDetailDto,
  FiscalYearDto,
};

export interface DisbursementRegisterFilters {
  fiscalYearId: number;
  fiscalPeriodId?: number;
  fundId?: number;
  status?: string;
  approverId?: number;
}
