import { z } from 'zod';

export const createItemCategorySchema = z.object({
  code: z.string().min(1, 'الرمز مطلوب').max(50),
  name: z.string().min(1, 'الاسم مطلوب').max(500),
  nameEn: z.string().max(500).optional().nullable(),
  description: z.string().optional().nullable(),
  parentItemCategoryId: z.number().optional().nullable(),
  expenseAccountId: z.number().optional().nullable(),
  inventoryAccountId: z.number().optional().nullable(),
  taxAccountId: z.number().optional().nullable(),
  taxClass: z.string().optional().nullable(),
  isActive: z.boolean().default(true),
});

export const updateItemCategorySchema = createItemCategorySchema;

export type CreateItemCategoryInput = z.infer<typeof createItemCategorySchema>;
export type UpdateItemCategoryInput = z.infer<typeof updateItemCategorySchema>;
