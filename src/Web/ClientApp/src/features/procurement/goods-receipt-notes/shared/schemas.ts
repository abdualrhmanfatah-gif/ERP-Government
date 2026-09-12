import { z } from 'zod';

export const grnLineSchema = z.object({
  purchaseOrderDetailId: z.number().min(1, 'تفاصيل أمر الشراء مطلوبة'),
  itemId: z.number().min(1, 'الصنف مطلوب'),
  unitId: z.number().min(1, 'الوحدة مطلوبة'),
  receivedQuantity: z.number().min(0.01, 'الكمية المستلمة يجب أن تكون > 0'),
  acceptedQuantity: z.number().min(0).optional().nullable(),
  rejectedQuantity: z.number().min(0).optional().nullable(),
  batchNumber: z.string().optional().nullable(),
  expiryDate: z.string().optional().nullable(),
  notes: z.string().optional().nullable(),
}).refine(
  (data) => {
    const accepted = data.acceptedQuantity ?? 0;
    const rejected = data.rejectedQuantity ?? 0;
    return accepted + rejected <= data.receivedQuantity;
  },
  { message: 'المقبولة + المرفوضة لا تتجاوز المستلمة', path: ['acceptedQuantity'] },
);

export const createGRNSchema = z.object({
  purchaseOrderId: z.number().min(1, 'أمر الشراء مطلوب'),
  warehouseId: z.number().min(1, 'المستودع مطلوب'),
  locationId: z.number().optional().nullable(),
  notes: z.string().optional().nullable(),
  lines: z.array(grnLineSchema).min(1, 'يجب إضافة بند واحد على الأقل'),
});

export const rejectGRNSchema = z.object({
  reason: z.string().min(1, 'سبب الرفض مطلوب'),
});

export type GRNLineFormData = z.infer<typeof grnLineSchema>;
export type CreateGRNFormData = z.infer<typeof createGRNSchema>;
export type RejectGRNFormData = z.infer<typeof rejectGRNSchema>;
