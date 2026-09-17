import { z } from 'zod';

export const accountSchema = z.object({
  code: z.string().min(1, 'الرمز مطلوب'),
  name: z.string().min(1, 'الاسم مطلوب'),
  description: z.string().optional(),
  accountGroupId: z.coerce.number().min(1, 'المجموعة مطلوبة'),
  parentId: z.coerce.number().optional().nullable(),
  normalBalance: z.coerce.number().min(0, 'نوع الحساب مطلوب'),
  isPostable: z.boolean(),
  isReconcilable: z.boolean(),
  currencyId: z.coerce.number().optional().nullable(),
});

export type AccountFormData = z.infer<typeof accountSchema>;

export const accountGroupSchema = z.object({
  code: z.string().min(1, 'الكود مطلوب').max(20, 'الكود 20 حرف كحد أقصى'),
  name: z.string().min(1, 'الاسم مطلوب').max(200),
  type: z.enum(['Asset', 'Liability', 'Equity', 'Revenue', 'Expense']),
  normalBalance: z.enum(['Debit', 'Credit']),
  description: z.string().max(500).optional().nullable(),
  parentId: z.number().nullable().optional(),
}).refine((d) => {
  const map: Record<string, string> = { Asset: 'Debit', Expense: 'Debit', Liability: 'Credit', Equity: 'Credit', Revenue: 'Credit' };
  return map[d.type] === d.normalBalance;
}, { message: 'نوع الحساب غير متوافق مع النوع', path: ['normalBalance'] });

export type AccountGroupFormData = z.infer<typeof accountGroupSchema>;
