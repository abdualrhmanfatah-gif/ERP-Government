import { z } from 'zod';

export const clearCheckSchema = z.object({
  clearedAt: z.string().min(1, 'تاريخ التحصيل مطلوب'),
});

export const bounceCheckSchema = z.object({
  bouncedAt: z.string().min(1, 'تاريخ الارتجاع مطلوب'),
  reason: z.string().optional(),
});

export const replaceCheckSchema = z.object({
  paymentMethod: z.enum(['Cash', 'Check'], {
    required_error: 'طريقة الدفع مطلوبة',
  }),
  voucherDate: z.string().min(1, 'تاريخ السند مطلوب'),
  checkDetails: z
    .object({
      bankName: z.string().min(1, 'اسم البنك مطلوب'),
      checkNumber: z.string().min(1, 'رقم الشيك مطلوب'),
      checkDate: z.string().min(1, 'تاريخ الشيك مطلوب'),
    })
    .optional(),
});

export type ClearCheckFormData = z.infer<typeof clearCheckSchema>;
export type BounceCheckFormData = z.infer<typeof bounceCheckSchema>;
export type ReplaceCheckFormData = z.infer<typeof replaceCheckSchema>;
