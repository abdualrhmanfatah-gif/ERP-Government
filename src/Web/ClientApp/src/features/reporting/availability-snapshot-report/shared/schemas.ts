import { z } from 'zod';

export const availabilitySnapshotFilterSchema = z.object({
  fiscalYearId: z.number({ invalid_type_error: 'السنة المالية مطلوبة' }).int().positive('السنة المالية مطلوبة'),
  budgetItemId: z.number().int().positive().optional(),
  fundId: z.number().int().positive().optional(),
});

export type AvailabilitySnapshotFilterFormData = z.infer<typeof availabilitySnapshotFilterSchema>;
