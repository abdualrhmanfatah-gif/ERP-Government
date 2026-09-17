import { z } from 'zod';

export const createAssetGroupSchema = z.object({
  code: z.string().min(1, 'كود المجموعة مطلوب').max(50, 'كود المجموعة يجب أن لا يتجاوز 50 حرف'),
  name: z.string().min(1, 'اسم المجموعة مطلوب').max(200, 'اسم المجموعة يجب أن لا يتجاوز 200 حرف'),
  description: z.string().max(500).optional().nullable(),
  parentAssetGroupId: z.number().optional().nullable(),
  assetCategory: z.string().min(1, 'فئة الأصول مطلوبة'),
  isDepreciable: z.boolean(),
  depreciationMethod: z.string().min(1, 'طريقة الإهلاك مطلوبة'),
  depreciationRate: z.number().min(0).optional().nullable(),
  defaultUsefulLifeYears: z.number().int().min(0).optional().nullable(),
  residualValuePercentage: z.number().min(0).max(100).optional().nullable(),
  assetAccountId: z.number().optional().nullable(),
  accumulatedDepreciationAccountId: z.number().optional().nullable(),
  depreciationExpenseAccountId: z.number().optional().nullable(),
  disposalAccountId: z.number().optional().nullable(),
});

export const updateAssetGroupSchema = z.object({
  name: z.string().min(1, 'اسم المجموعة مطلوب').max(200, 'اسم المجموعة يجب أن لا يتجاوز 200 حرف'),
  description: z.string().max(500).optional().nullable(),
  parentAssetGroupId: z.number().optional().nullable(),
  assetCategory: z.string().min(1, 'فئة الأصول مطلوبة'),
  isDepreciable: z.boolean(),
  depreciationMethod: z.string().min(1, 'طريقة الإهلاك مطلوبة'),
  depreciationRate: z.number().min(0).optional().nullable(),
  defaultUsefulLifeYears: z.number().int().min(0).optional().nullable(),
  residualValuePercentage: z.number().min(0).max(100).optional().nullable(),
  assetAccountId: z.number().optional().nullable(),
  accumulatedDepreciationAccountId: z.number().optional().nullable(),
  depreciationExpenseAccountId: z.number().optional().nullable(),
  disposalAccountId: z.number().optional().nullable(),
  rowVersion: z.string().min(1, 'RowVersion مطلوب'),
});

export type CreateAssetGroupInput = z.infer<typeof createAssetGroupSchema>;
export type UpdateAssetGroupInput = z.infer<typeof updateAssetGroupSchema>;
