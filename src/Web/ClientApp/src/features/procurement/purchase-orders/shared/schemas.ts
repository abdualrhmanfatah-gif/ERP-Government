import { z } from 'zod';

export const purchaseOrderLineSchema = z.object({
  id: z.number().optional(),
  purchaseRequestDetailId: z.number().optional().nullable(),
  quotationDetailId: z.number().optional().nullable(),
  itemId: z.number().min(1, 'الصنف مطلوب'),
  unitId: z.number().min(1, 'الوحدة مطلوبة'),
  orderedQuantity: z.number().min(0.01, 'الكمية يجب أن تكون > 0'),
  unitPrice: z.number().min(0, 'سعر الوحدة يجب أن يكون >= 0'),
  discountPercent: z.number().min(0).max(100).optional().nullable(),
  taxPercent: z.number().min(0).max(100).optional().nullable(),
  expectedDeliveryDate: z.string().optional().nullable(),
  notes: z.string().optional().nullable(),
});

export const createPurchaseOrderSchema = z.object({
  purchaseRequestId: z.number().optional().nullable(),
  quotationId: z.number().optional().nullable(),
  supplierPartyId: z.number().min(1, 'المورد مطلوب'),
  warehouseId: z.number().optional().nullable(),
  deliveryLocationId: z.number().optional().nullable(),
  currencyCode: z.string().optional().nullable(),
  exchangeRate: z.number().optional().nullable(),
  paymentTerms: z.string().optional().nullable(),
  deliveryTerms: z.string().optional().nullable(),
  expectedDeliveryDate: z.string().optional().nullable(),
  notes: z.string().optional().nullable(),
  lines: z.array(purchaseOrderLineSchema).min(1, 'يجب إضافة بند واحد على الأقل'),
});

export const updatePurchaseOrderSchema = z.object({
  paymentTerms: z.string().optional().nullable(),
  deliveryTerms: z.string().optional().nullable(),
  expectedDeliveryDate: z.string().optional().nullable(),
  notes: z.string().optional().nullable(),
  lines: z.array(purchaseOrderLineSchema).min(1, 'يجب إضافة بند واحد على الأقل'),
});

export type PurchaseOrderLineFormData = z.infer<typeof purchaseOrderLineSchema>;
export type CreatePurchaseOrderFormData = z.infer<typeof createPurchaseOrderSchema>;
export type UpdatePurchaseOrderFormData = z.infer<typeof updatePurchaseOrderSchema>;
