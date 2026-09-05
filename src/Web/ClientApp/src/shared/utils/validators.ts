import { z } from 'zod';

/**
 * Common ERP validation schemas.
 * Arabic error messages.
 */

export const requiredString = (label: string) =>
  z.string().min(1, `${label} مطلوب`);

export const positiveNumber = (label: string) =>
  z.number().positive(`${label} يجب أن يكون أكبر من صفر`);

export const decimalPrecision = (label: string, maxDecimals: number) =>
  z.number().refine(
    (val) => {
      const parts = val.toString().split('.');
      return !parts[1] || parts[1].length <= maxDecimals;
    },
    { message: `${label} لا يمكن أن يحتوي على أكثر من ${maxDecimals} خانات عشرية` }
  );

export const emailSchema = z
  .string()
  .min(1, 'البريد الإلكتروني مطلوب')
  .email('البريد الإلكتروني غير صالح');

export const passwordSchema = z
  .string()
  .min(6, 'كلمة المرور يجب أن تكون 6 أحرف على الأقل');

export const confirmPasswordSchema = (label: string) =>
  z.string().min(1, `${label} مطلوب`);

export const optionalString = z.string().optional();
