import { z } from 'zod';

const deductionSchema = z.object({
  deductionType: z.number().min(0),
  deductionCode: z.string().optional(),
  description: z.string().optional(),
  accountId: z.number().min(1, 'الحساب مطلوب'),
  amount: z.number().min(0, 'المبلغ يجب أن يكون ≥ 0'),
  deductionPercent: z.number().optional(),
  isMandatory: z.boolean().optional(),
  isTaxDeduction: z.boolean().optional(),
  taxAuthorityId: z.number().optional(),
  referenceNumber: z.string().optional(),
});

export const createPaymentOrderSchema = z.object({
  paymentOrderDate: z.string().min(1, 'التاريخ مطلوب'),
  dueDate: z.string().optional(),
  paymentOrderType: z.string().min(1, 'نوع أمر الدفع مطلوب').max(20),
  accountId: z.number().optional(),
  disbursementRequestId: z.number().optional(),
  fundId: z.number().min(1, 'الصندوق مطلوب'),
  fiscalYearId: z.number().optional(),
  budgetItemAllocationId: z.number().optional(),
  budgetClassificationId: z.number().optional(),
  costCenterId: z.number().optional(),
  purchaseOrderId: z.number().optional(),
  encumbranceId: z.number().optional(),
  currencyId: z.number().min(1, 'العملة مطلوبة'),
  exchangeRate: z.number().optional(),
  amountGross: z.number().min(0.01, 'المبلغ الإجمالي يجب أن يكون > 0'),
  deductionAmount: z.number().min(0, 'لا يمكن أن يكون سالباً'),
  paymentMethod: z.string().optional(),
  bankAccountId: z.number().optional(),
  beneficiaryName: z.string().min(1, 'اسم المستفيد مطلوب').max(200),
  beneficiaryAccountNumber: z.string().optional(),
  beneficiaryBankName: z.string().optional(),
  notes: z.string().optional(),
  deductions: z.array(deductionSchema),
}).refine(
  (data) => {
    if (data.paymentMethod === 'Check' && (!data.bankAccountId || data.bankAccountId <= 0)) {
      return false;
    }
    return true;
  },
  { message: 'الحساب البنكي مطلوب عند الدفع بشيك', path: ['bankAccountId'] },
);

export const updatePaymentOrderSchema = z.object({
  paymentOrderDate: z.string().min(1, 'التاريخ مطلوب'),
  dueDate: z.string().optional(),
  paymentOrderType: z.string().min(1, 'نوع أمر الدفع مطلوب').max(20),
  accountId: z.number().nullish(),
  fundId: z.number().nullish(),
  fiscalYearId: z.number().nullish(),
  budgetItemAllocationId: z.number().nullish(),
  budgetClassificationId: z.number().nullish(),
  costCenterId: z.number().nullish(),
  purchaseOrderId: z.number().nullish(),
  encumbranceId: z.number().nullish(),
  currencyId: z.number().nullish(),
  exchangeRate: z.number().nullish(),
  amountGross: z.number().min(0.01, 'المبلغ الإجمالي يجب أن يكون > 0'),
  deductionAmount: z.number().min(0, 'لا يمكن أن يكون سالباً'),
  paymentMethod: z.string().nullish(),
  bankAccountId: z.number().nullish(),
  beneficiaryName: z.string().min(1, 'اسم المستفيد مطلوب').max(200),
  beneficiaryAccountNumber: z.string().nullish(),
  beneficiaryBankName: z.string().nullish(),
  notes: z.string().nullish(),
  deductions: z.array(deductionSchema),
});

export type CreatePaymentOrderFormData = z.infer<typeof createPaymentOrderSchema>;
