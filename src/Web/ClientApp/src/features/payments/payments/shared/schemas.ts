import { z } from 'zod';
import { PaymentMethod } from './types';

export const recordPaymentSchema = z.object({
  paymentOrderId: z.number().min(1, 'أمر الدفع مطلوب'),
  paymentMethod: z.nativeEnum(PaymentMethod, { error_map: () => ({ message: 'طريقة الدفع مطلوبة' }) }),
  referenceNumber: z.string().optional(),
  notes: z.string().optional(),
});

export type RecordPaymentFormData = z.infer<typeof recordPaymentSchema>;
