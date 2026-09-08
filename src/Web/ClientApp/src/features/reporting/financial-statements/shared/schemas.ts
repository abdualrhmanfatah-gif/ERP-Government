import { z } from 'zod';

export const balanceSheetFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  asOfDate: z.string().min(1, 'تاريخ الرصيد مطلوب'),
  fiscalPeriodId: z.number().int().positive().optional(),
});

export const incomeStatementFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  startDate: z.string().min(1, 'تاريخ البداية مطلوب'),
  endDate: z.string().min(1, 'تاريخ النهاية مطلوب'),
});

export const cashFlowStatementFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  startDate: z.string().min(1, 'تاريخ البداية مطلوب'),
  endDate: z.string().min(1, 'تاريخ النهاية مطلوب'),
});

export const generalLedgerFilterSchema = z.object({
  accountId: z.number().int().positive().optional(),
  accountCode: z.string().optional(),
  fiscalPeriodId: z.number().int().positive().optional(),
  fiscalYearId: z.number().int().positive().optional(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  page: z.number().int().min(0).optional(),
  pageSize: z.number().int().min(1).max(500).optional(),
});

export type GeneralLedgerFilterFormData = z.infer<typeof generalLedgerFilterSchema>;
