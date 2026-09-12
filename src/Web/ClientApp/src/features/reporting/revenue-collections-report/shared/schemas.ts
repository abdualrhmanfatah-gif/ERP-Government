import { z } from 'zod';

export const revenueCollectionsFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  fiscalPeriodId: z.number().int().positive().optional(),
  fundId: z.number().int().positive().optional(),
  revenueAccountId: z.number().int().positive().optional(),
  partyId: z.number().int().positive().optional(),
  paymentMethod: z.string().optional(),
});

export type RevenueCollectionsFilterFormData = z.infer<typeof revenueCollectionsFilterSchema>;
