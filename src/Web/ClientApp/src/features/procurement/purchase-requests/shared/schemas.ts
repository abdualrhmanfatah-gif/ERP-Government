import { z } from 'zod';

const purchaseRequestLineSchema = z.object({
  itemId: z.number().min(1, 'الصنف مطلوب'),
  unitId: z.number().min(1, 'الوحدة مطلوبة'),
  requestedQuantity: z.number().min(0.01, 'الكمية يجب أن تكون > 0'),
  unitCostEstimate: z.number().min(0).optional().nullable(),
  notes: z.string().optional().nullable(),
});

export const createPurchaseRequestSchema = z.object({
  requestDate: z.string().min(1, 'التاريخ مطلوب'),
  requiredDate: z.string().optional().nullable(),
  departmentId: z.number().optional().nullable(),
  costCenterId: z.number().optional().nullable(),
  requesterName: z.string().min(1, 'اسم مقدم الطلب مطلوب').max(200, 'اسم مقدم الطلب يجب ألا يتجاوز 200 حرف'),
  priority: z.enum(['Low', 'Normal', 'High', 'Urgent'], {
    required_error: 'الأولوية مطلوبة',
  }),
  notes: z.string().optional().nullable(),
  lines: z
    .array(purchaseRequestLineSchema)
    .min(1, 'يجب إضافة بند واحد على الأقل'),
});

export const updatePurchaseRequestSchema = z.object({
  requestDate: z.string().min(1, 'التاريخ مطلوب'),
  requiredDate: z.string().optional().nullable(),
  departmentId: z.number().optional().nullable(),
  costCenterId: z.number().optional().nullable(),
  requesterName: z.string().min(1, 'اسم مقدم الطلب مطلوب').max(200, 'اسم مقدم الطلب يجب ألا يتجاوز 200 حرف'),
  priority: z.enum(['Low', 'Normal', 'High', 'Urgent'], {
    required_error: 'الأولوية مطلوبة',
  }),
  notes: z.string().optional().nullable(),
  lines: z
    .array(purchaseRequestLineSchema)
    .min(1, 'يجب إضافة بند واحد على الأقل'),
});

export type CreatePurchaseRequestFormData = z.infer<typeof createPurchaseRequestSchema>;
export type UpdatePurchaseRequestFormData = z.infer<typeof updatePurchaseRequestSchema>;
export type PurchaseRequestLineFormData = z.infer<typeof purchaseRequestLineSchema>;
