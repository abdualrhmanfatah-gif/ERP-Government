import { z } from 'zod';

const quotationLineSchema = z.object({
  purchaseRequestDetailId: z.number().min(1, 'بند طلب الشراء مطلوب'),
  itemId: z.number().min(1, 'الصنف مطلوب'),
  unitId: z.number().min(1, 'الوحدة مطلوبة'),
  quantity: z.number().min(0.01, 'الكمية يجب أن تكون > 0'),
  unitPrice: z.number().min(0, 'سعر الوحدة يجب أن يكون ≥ 0'),
  discountPercent: z.number().min(0).max(100).optional().nullable(),
  taxPercent: z.number().min(0).max(100).optional().nullable(),
  notes: z.string().optional().nullable(),
});

export const createQuotationSchema = z.object({
  supplierPartyId: z.number().min(1, 'المورد مطلوب'),
  quotationDate: z.string().min(1, 'تاريخ العرض مطلوب'),
  validUntil: z.string().optional().nullable(),
  currencyCode: z.string().optional().nullable(),
  exchangeRate: z.number().optional().nullable(),
  shippingCost: z.number().optional().nullable(),
  otherCharges: z.number().optional().nullable(),
  paymentTerms: z.string().optional().nullable(),
  deliveryTerms: z.string().optional().nullable(),
  leadTimeDays: z.number().optional().nullable(),
  warrantyPeriodMonths: z.number().optional().nullable(),
  notes: z.string().optional().nullable(),
  lines: z
    .array(quotationLineSchema)
    .min(1, 'يجب إضافة بند واحد على الأقل'),
});

export const updateQuotationSchema = createQuotationSchema;

export type CreateQuotationFormData = z.infer<typeof createQuotationSchema>;
export type UpdateQuotationFormData = z.infer<typeof updateQuotationSchema>;
export type QuotationLineFormData = z.infer<typeof quotationLineSchema>;
