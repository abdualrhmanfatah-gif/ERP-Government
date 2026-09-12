import type {
  TrialBalanceReportDto,
  TrialBalanceLineDto,
  TrialBalanceTotalDto,
  LedgerMovementDto,
  LedgerMovementLineDto,
  LedgerMovementTotalDto,
  FiscalYearDto,
  FiscalPeriodDto,
} from '@/web-api-client';

export type {
  TrialBalanceReportDto,
  TrialBalanceLineDto,
  TrialBalanceTotalDto,
  LedgerMovementDto,
  LedgerMovementLineDto,
  LedgerMovementTotalDto,
  FiscalYearDto,
  FiscalPeriodDto,
};

export interface TrialBalanceFilters {
  fiscalYearId: number;
  fiscalPeriodId?: number;
}
