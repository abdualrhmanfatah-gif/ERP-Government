import { z } from 'zod';

export const createWarehouseSchema = z.object({
  code: z.string().min(1, 'الرمز مطلوب').max(50),
  name: z.string().min(1, 'الاسم مطلوب').max(500),
  locationId: z.number().optional().nullable(),
  managerId: z.number().optional().nullable(),
  address: z.string().optional().nullable(),
  city: z.string().optional().nullable(),
  phone: z.string().optional().nullable(),
  email: z.string().email('بريد إلكتروني غير صحيح').optional().nullable().or(z.literal('')),
  totalCapacity: z.number().min(0).optional().nullable(),
  currentLoad: z.number().min(0).optional().nullable(),
  isActive: z.boolean().default(true),
}).refine(
  (data) => {
    if (data.currentLoad != null && data.totalCapacity != null) {
      return data.currentLoad <= data.totalCapacity;
    }
    return true;
  },
  { message: 'الحمل الحالي لا يجب أن يتجاوز السعة الإجمالية', path: ['currentLoad'] }
);

export const updateWarehouseSchema = createWarehouseSchema;

export type CreateWarehouseInput = z.infer<typeof createWarehouseSchema>;
export type UpdateWarehouseInput = z.infer<typeof updateWarehouseSchema>;
