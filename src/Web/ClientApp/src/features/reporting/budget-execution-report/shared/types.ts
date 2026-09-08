import type {
  BudgetExecutionReportDto,
  BudgetExecutionLineDto,
  BudgetExecutionTotalDto,
  BudgetExecutionDetailDto,
  EncumbranceDetailDto,
  PaymentDetailDto,
  FiscalYearDto,
} from '@/web-api-client';

export type {
  BudgetExecutionReportDto,
  BudgetExecutionLineDto,
  BudgetExecutionTotalDto,
  BudgetExecutionDetailDto,
  EncumbranceDetailDto,
  PaymentDetailDto,
  FiscalYearDto,
};

export interface BudgetExecutionFilters {
  fiscalYearId: number;
  fundId?: number;
  programId?: number;
  projectId?: number;
  budgetItemId?: number;
}

export const PAGINATION_THRESHOLD = 500;

export function computeUsageRatio(
  paidAmount: number,
  appropriatedAmount: number,
): number | null {
  if (!appropriatedAmount) return null;
  return paidAmount / appropriatedAmount;
}
