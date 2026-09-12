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

export const statusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Submitted: 'مُقدّم',
  Approved: 'معتمد',
  Paid: 'مدفوع',
  Rejected: 'مرفوض',
};
