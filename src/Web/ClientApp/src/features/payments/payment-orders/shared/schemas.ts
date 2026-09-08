import { z } from 'zod';

const lineSchema = z.object({
  lineType: z.number().min(0),
  description: z.string().optional(),
  accountId: z.number().min(1, 'الحساب مطلوب'),
  amount: z.number().min(0, 'المبلغ يجب أن يكون ≥ 0'),
  taxAmount: z.number().optional(),
  fundId: z.number().optional(),
  appropriationId: z.number().optional(),
  organizationUnitId: z.number().optional(),
  costCenterId: z.number().optional(),
  projectId: z.number().optional(),
});

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
  vendorId: z.number().min(1, 'المورد مطلوب'),
  fundId: z.number().min(1, 'الصندوق مطلوب'),
  fiscalYearId: z.number().min(1, 'السنة المالية مطلوبة'),
  appropriationId: z.number().min(1, 'التخصيص مطلوب'),
  budgetClassificationId: z.number().optional(),
  costCenterId: z.number().optional(),
  projectId: z.number().optional(),
  purchaseOrderId: z.number().optional(),
  encumbranceId: z.number().optional(),
  currencyId: z.number().min(1, 'العملة مطلوبة'),
  exchangeRate: z.number().optional(),
  amountGross: z.number().min(0.01, 'المبلغ الإجمالي يجب أن يكون > 0'),
  deductionAmount: z.number().min(0, 'لا يمكن أن يكون سالباً'),
  paymentMethod: z.number().optional(),
  bankAccountId: z.number().optional(),
  beneficiaryName: z.string().min(1, 'اسم المستفيد مطلوب').max(200),
  beneficiaryIban: z.string().optional(),
  beneficiaryAccountNumber: z.string().optional(),
  beneficiaryBankName: z.string().optional(),
  notes: z.string().optional(),
  lines: z.array(lineSchema).min(1, 'يجب إضافة سطر واحد على الأقل'),
  deductions: z.array(deductionSchema),
}).refine(
  (data) => {
    const net = data.amountGross - data.deductionAmount;
    const linesSum = data.lines.reduce((sum, l) => sum + l.amount, 0);
    return Math.abs(linesSum - net) <= 0.01;
  },
  { message: 'مجمل الأسطر يجب أن يساوي المبلغ الصافي', path: ['lines'] },
).refine(
  (data) => {
    const deductionsSum = data.deductions.reduce((sum, d) => sum + d.amount, 0);
    return Math.abs(deductionsSum - data.deductionAmount) <= 0.01;
  },
  { message: 'مجمل الخصومات يجب أن يساوي مبلغ الخصم', path: ['deductions'] },
);

export type CreatePaymentOrderFormData = z.infer<typeof createPaymentOrderSchema>;
