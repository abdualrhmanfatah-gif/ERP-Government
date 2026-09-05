import { z } from 'zod';

/**
 * Financial Settings validation schemas.
 * Arabic error messages per Design System Protocol.
 */

// ─── Currency ──────────────────────────────────────────────────────

export const currencySchema = z.object({
  code: z
    .string()
    .min(1, 'كود العملة مطلوب')
    .max(10, 'كود العملة لا يمكن أن يتجاوز 10 أحرف'),
  name: z
    .string()
    .min(1, 'اسم العملة مطلوب')
    .max(100, 'اسم العملة لا يمكن أن يتجاوز 100 حرف'),
  symbol: z
    .string()
    .min(1, 'رمز العملة مطلوب')
    .max(10, 'رمز العملة لا يمكن أن يتجاوز 10 أحرف'),
  decimalPlaces: z
    .number()
    .int()
    .min(0, 'الحالات العشرية يجب أن تكون 0 على الأقل')
    .max(6, 'الحالات العشرية لا يمكن أن تتجاوز 6')
    .default(2),
  roundingPrecision: z
    .number()
    .positive('دقة التحديد يجب أن تكون أكبر من صفر')
    .default(0.01),
  isBase: z.boolean().default(false),
});

export type CurrencyFormData = z.infer<typeof currencySchema>;

// ─── Exchange Rate ─────────────────────────────────────────────────

export const exchangeRateSchema = z
  .object({
    baseCurrencyId: z.number().min(1, 'العملة الأساسية مطلوبة'),
    currencyId: z.number().min(1, 'العملة الهدف مطلوبة'),
    rateDate: z.date({ required_error: 'تاريخ السعر مطلوب' }),
    rateType: z.enum(['Official', 'Market'], {
      required_error: 'نوع السعر مطلوب',
    }),
    rate: z.number().positive('يجب أن يكون سعر الصرف أكبر من صفر'),
  })
  .refine((data) => data.baseCurrencyId !== data.currencyId, {
    message: 'يجب أن تكون العملتان مختلفتين',
    path: ['currencyId'],
  });

export type ExchangeRateFormData = z.infer<typeof exchangeRateSchema>;

// ─── Fiscal Year ───────────────────────────────────────────────────

export const fiscalYearSchema = z
  .object({
    name: z
      .string()
      .min(1, 'اسم السنة المالية مطلوب')
      .max(100, 'اسم السنة المالية لا يمكن أن يتجاوز 100 حرف'),
    yearNumber: z
      .number()
      .int()
      .positive('رقم السنة يجب أن يكون أكبر من صفر'),
    startDate: z.date({ required_error: 'تاريخ البداية مطلوب' }),
    endDate: z.date({ required_error: 'تاريخ النهاية مطلوب' }),
  })
  .refine((data) => data.endDate > data.startDate, {
    message: 'تاريخ النهاية يجب أن يكون بعد تاريخ البداية',
    path: ['endDate'],
  });

export type FiscalYearFormData = z.infer<typeof fiscalYearSchema>;

// ─── Fiscal Period ─────────────────────────────────────────────────

export const fiscalPeriodSchema = z
  .object({
    name: z
      .string()
      .min(1, 'اسم الفترة مطلوب')
      .max(100, 'اسم الفترة لا يمكن أن يتجاوز 100 حرف'),
    periodNumber: z
      .number()
      .int()
      .positive('رقم الفترة يجب أن يكون أكبر من صفر'),
    startDate: z.date({ required_error: 'تاريخ البداية مطلوب' }),
    endDate: z.date({ required_error: 'تاريخ النهاية مطلوب' }),
  })
  .refine((data) => data.endDate > data.startDate, {
    message: 'تاريخ النهاية يجب أن يكون بعد تاريخ البداية',
    path: ['endDate'],
  });

export type FiscalPeriodFormData = z.infer<typeof fiscalPeriodSchema>;

// ─── Document Sequence ─────────────────────────────────────────────

export const documentSequenceSchema = z.object({
  name: z
    .string()
    .min(1, 'اسم التسلسل مطلوب')
    .max(100, 'اسم التسلسل لا يمكن أن يتجاوز 100 حرف'),
  documentType: z.string().min(1, 'نوع المستند مطلوب'),
  fiscalYearId: z.number().min(1, 'السنة المالية مطلوبة'),
  resetPolicy: z.enum(['Yearly', 'Never'], {
    required_error: 'سياسة إعادة التعيين مطلوبة',
  }),
});

export type DocumentSequenceFormData = z.infer<typeof documentSequenceSchema>;
