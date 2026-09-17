import { z } from 'zod';

export const attributeDefinitionSchema = z.object({
  code: z.string().min(1, 'كود المواصفة مطلوب').max(50, 'كود المواصفة يجب أن لا يتجاوز 50 حرف'),
  name: z.string().min(1, 'اسم المواصفة مطلوب').max(200, 'اسم المواصفة يجب أن لا يتجاوز 200 حرف'),
  description: z.string().max(500, 'الوصف يجب أن لا يتجاوز 500 حرف').optional().nullable(),
  unit: z.string().max(50, 'الوحدة يجب أن لا تتجاوز 50 حرف').optional().nullable(),
  sortOrder: z.number().int('ترتيب العرض يجب أن يكون رقماً صحيحاً').min(0, 'ترتيب العرض يجب أن يكون صفراً أو أكثر'),
  attributeDataType: z.enum(['Text', 'Integer', 'Decimal', 'Date', 'Boolean']),
  isActive: z.boolean(),
});

export type AttributeDefinitionInput = z.infer<typeof attributeDefinitionSchema>;
