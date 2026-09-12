import { z } from 'zod';

export const createDisbursementRequestSchema = z.object({
  beneficiaryName: z.string().trim().min(1, 'اسم المستفيد مطلوب').max(200),
  beneficiaryPartyId: z.number().positive().optional().nullable(),
  requestedAmount: z.number().positive('المبلغ المطلوب يجب أن يكون أكبر من صفر').max(9999999999999.99, 'المبلغ يتجاوز الحد الأقصى'),
  currencyId: z.number().positive('العملة مطلوبة'),
  purpose: z.string().trim().min(1, 'الغرض مطلوب').max(500),
  financialYearId: z.number().positive('السنة المالية مطلوبة'),
  notes: z.string().optional(),
});

export type CreateDisbursementRequestFormData = z.infer<typeof createDisbursementRequestSchema>;

export const approveDisbursementRequestSchema = z.object({
  approvedAmount: z.number().positive('المبلغ المعتمد يجب أن يكون أكبر من صفر'),
  issuingAuthorityName: z.string().trim().min(1, 'اسم جهة الأمر مطلوب'),
  issuingAuthorityCapacity: z.string().trim().min(1, 'صفة جهة الأمر مطلوبة'),
});

export type ApproveDisbursementRequestFormData = z.infer<typeof approveDisbursementRequestSchema>;

export const rejectDisbursementRequestSchema = z.object({
  reason: z.string().trim().min(1, 'سبب الرفض مطلوب'),
});

export type RejectDisbursementRequestFormData = z.infer<typeof rejectDisbursementRequestSchema>;

export const cancelDisbursementRequestSchema = z.object({
  reason: z.string().trim().min(1, 'سبب الإلغاء مطلوب'),
});

export type CancelDisbursementRequestFormData = z.infer<typeof cancelDisbursementRequestSchema>;

export const createAccrualEntrySchema = z.object({
  expenseAccountId: z.number().positive('حساب المصروف مطلوب'),
  liabilityAccountId: z.number().positive('حساب الخصم مطلوب'),
  amount: z.number().positive('المبلغ يجب أن يكون أكبر من صفر'),
  currencyId: z.number().positive('العملة مطلوبة'),
  costCenterId: z.number().positive().optional().nullable(),
  narration: z.string().optional(),
});

export type CreateAccrualEntryFormData = z.infer<typeof createAccrualEntrySchema>;
