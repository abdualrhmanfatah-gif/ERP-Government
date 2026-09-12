import { z } from 'zod';

const nonNegative = z.number().min(0).optional().nullable();

export const createItemSchema = z.object({
  name: z.string().min(1, 'الاسم مطلوب').max(500),
  nameEn: z.string().max(500).optional().nullable(),
  description: z.string().optional().nullable(),
  categoryId: z.number().optional().nullable(),
  unitId: z.number().min(1, 'الوحدة مطلوبة'),
  supplierId: z.number().optional().nullable(),
  barcode: z.string().max(100).optional().nullable(),
  itemType: z.string().min(1, 'نوع الصنف مطلوب'),
  openingStock: nonNegative,
  minimumStock: nonNegative,
  maximumStock: nonNegative,
  reorderLevel: nonNegative,
  reorderQuantity: nonNegative,
  leadTimeDays: z.number().int().min(0).optional().nullable(),
  isActive: z.boolean(),
}).refine(
  (data) => {
    if (data.minimumStock != null && data.maximumStock != null) {
      return data.minimumStock <= data.maximumStock;
    }
    return true;
  },
  { message: 'الحد الأدنى لا يجب أن يتجاوز الحد الأقصى', path: ['maximumStock'] }
);

export const updateItemSchema = createItemSchema;

export type CreateItemInput = z.infer<typeof createItemSchema>;
export type UpdateItemInput = z.infer<typeof updateItemSchema>;
