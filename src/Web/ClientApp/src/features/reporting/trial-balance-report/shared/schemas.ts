import { z } from 'zod';

export const trialBalanceFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  fiscalPeriodId: z.number().int().positive().optional(),
});

export type TrialBalanceFilterFormData = z.infer<typeof trialBalanceFilterSchema>;
