import { z } from 'zod';

export const transferFormSchema = z
  .object({
    assetId: z
      .number({ required_error: 'الأصل مطلوب', invalid_type_error: 'الأصل مطلوب' })
      .min(1, 'الأصل مطلوب'),
    transactionDate: z.string().min(1, 'تاريخ النقل مطلوب'),
    toLocationId: z.number().nullable().optional(),
    toEmployeeId: z.number().nullable().optional(),
    notes: z.string().max(2000, 'الملاحظات يجب أن لا تتجاوز 2000 حرف').optional(),
  })
  .refine((data) => data.toLocationId != null || data.toEmployeeId != null, {
    message: 'حدد وجهة النقل: موقع أو حارس جديد على الأقل',
    path: ['toLocationId'],
  });

export type TransferFormInput = z.infer<typeof transferFormSchema>;
