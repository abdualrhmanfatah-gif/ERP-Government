import { z } from 'zod';

export const budgetExecutionFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  fundId: z.number().int().positive().optional(),
  programId: z.number().int().positive().optional(),
  projectId: z.number().int().positive().optional(),
  budgetItemId: z.number().int().positive().optional(),
});

export type BudgetExecutionFilterFormData = z.infer<typeof budgetExecutionFilterSchema>;
