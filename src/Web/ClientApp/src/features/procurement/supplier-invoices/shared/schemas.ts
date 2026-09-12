import { z } from 'zod';

export const supplierInvoiceLineSchema = z.object({
  purchaseOrderDetailId: z.number().min(1, 'تفاصيل أمر الشراء مطلوبة'),
  itemId: z.number().min(1, 'الصنف مطلوب'),
  quantity: z.number().min(0.01, 'الكمية يجب أن تكون > 0'),
  unitPrice: z.number().min(0, 'سعر الوحدة يجب أن يكون >= 0'),
  discountAmount: z.number().min(0).optional().nullable(),
  taxAmount: z.number().min(0).optional().nullable(),
  notes: z.string().optional().nullable(),
});

export const createSupplierInvoiceSchema = z.object({
  purchaseOrderId: z.number().min(1, 'أمر الشراء مطلوب'),
  supplierInvoiceNumber: z.string().min(1, 'رقم فاتورة المورد مطلوب'),
  invoiceDate: z.string().min(1, 'تاريخ الفاتورة مطلوب'),
  currencyCode: z.string().optional().nullable(),
  exchangeRate: z.number().min(0).optional().nullable(),
  dueDate: z.string().optional().nullable(),
  notes: z.string().optional().nullable(),
  lines: z.array(supplierInvoiceLineSchema).min(1, 'يجب إضافة بند واحد على الأقل'),
});

export type SupplierInvoiceLineFormData = z.infer<typeof supplierInvoiceLineSchema>;
export type CreateSupplierInvoiceFormData = z.infer<typeof createSupplierInvoiceSchema>;
