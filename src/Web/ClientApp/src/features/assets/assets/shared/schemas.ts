import { z } from 'zod';

export const createAssetSchema = z.object({
  name: z.string().min(1, 'اسم الأصل مطلوب').max(200, 'اسم الأصل يجب أن لا يتجاوز 200 حرف'),
  description: z.string().max(500, 'الوصف يجب أن لا يتجاوز 500 حرف').optional(),
  assetGroupId: z.number({ required_error: 'المجموعة مطلوبة', invalid_type_error: 'المجموعة مطلوبة' }).min(1, 'المجموعة مطلوبة'),
  locationId: z.number().optional(),
  employeeId: z.number().optional(),
  assetTag: z.string().max(50).optional(),
  barcode: z.string().max(100).optional(),
  serialNumber: z.string().max(100).optional(),
  currencyId: z.number().min(1),
  exchangeRateId: z.number().optional(),
  originalValue: z.number({ required_error: 'القيمة الأصلية مطلوبة', invalid_type_error: 'القيمة الأصلية مطلوبة' }).min(0, 'القيمة الأصلية مطلوبة'),
  acquisitionCost: z.number().min(0).optional(),
  purchaseDate: z.string().min(1, 'تاريخ الشراء مطلوب'),
  depreciationStartDate: z.string().min(1, 'تاريخ بدء الإهلاك مطلوب'),
  acquisitionType: z.string().min(1, 'نوع الاستحواذ مطلوب'),
  usefulLifeYears: z.number().int().min(1).optional(),
  notes: z.string().max(2000).optional(),
});

export const updateAssetSchema = createAssetSchema.extend({
  status: z.string().optional(),
  rowVersion: z.string().min(1, 'إصدار السطر مطلوب'),
});

export type CreateAssetInput = z.infer<typeof createAssetSchema>;
export type UpdateAssetInput = z.infer<typeof updateAssetSchema>;
