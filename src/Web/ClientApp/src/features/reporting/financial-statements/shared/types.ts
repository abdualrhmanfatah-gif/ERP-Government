import type {
  BalanceSheetDto,
  IncomeStatementDto,
  CashFlowStatementDto,
  GeneralLedgerDto,
  GeneralLedgerLine,
  GeneralLedgerTotals,
  TrialBalanceReportDto,
  TrialBalanceLineDto,
  TrialBalanceTotalDto,
  FiscalYearDto,
  FiscalPeriodDto,
} from '@/web-api-client';

export type {
  BalanceSheetDto,
  IncomeStatementDto,
  CashFlowStatementDto,
  GeneralLedgerDto,
  GeneralLedgerLine,
  GeneralLedgerTotals,
  TrialBalanceReportDto,
  TrialBalanceLineDto,
  TrialBalanceTotalDto,
  FiscalYearDto,
  FiscalPeriodDto,
};

export interface FinancialStatementFilters {
  fiscalYearId: number;
  fiscalPeriodId?: number;
  fiscalPeriodEndId?: number;
  asOfDate?: string;
  startDate?: string;
  endDate?: string;
}

export interface GeneralLedgerFilters {
  accountId?: number;
  accountCode?: string;
  fiscalPeriodId?: number;
  fiscalYearId?: number;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}
