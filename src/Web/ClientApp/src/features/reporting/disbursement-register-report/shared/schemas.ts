import { z } from 'zod';

export const disbursementRegisterFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  fiscalPeriodId: z.number().int().positive().optional(),
  fundId: z.number().int().positive().optional(),
  status: z.string().optional(),
  approverId: z.number().int().positive().optional(),
});

export type DisbursementRegisterFilterFormData = z.infer<typeof disbursementRegisterFilterSchema>;
