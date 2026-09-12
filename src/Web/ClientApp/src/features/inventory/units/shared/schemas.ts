import { z } from 'zod';

export const createUnitSchema = z.object({
  code: z.string().min(1, 'الرمز مطلوب').max(50),
  name: z.string().min(1, 'الاسم مطلوب').max(500),
  nameAr: z.string().max(500).optional().nullable(),
  unitType: z.string().optional().nullable(),
  baseUnitId: z.number().optional().nullable(),
  conversionToBase: z.number().optional().nullable(),
  isActive: z.boolean().default(true),
}).refine(
  (data) => {
    if (data.baseUnitId && (!data.conversionToBase || data.conversionToBase <= 0)) {
      return false;
    }
    return true;
  },
  { message: 'معامل التحويل يجب أن يكون أكبر من 0 عند اختيار وحدة أساسية', path: ['conversionToBase'] }
);

export const updateUnitSchema = createUnitSchema;

export type CreateUnitInput = z.infer<typeof createUnitSchema>;
export type UpdateUnitInput = z.infer<typeof updateUnitSchema>;
