export type SupplierInvoiceStatus = 'Draft' | 'Submitted' | 'Matched' | 'PartiallyPaid' | 'Paid' | 'Disputed' | 'Cancelled';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface SupplierInvoice {
  id: number;
  invoiceNumber: string;
  supplierInvoiceNumber: string;
  invoiceDate: string;
  purchaseOrderId: number;
  purchaseOrderNumber?: string;
  supplierPartyId: number;
  supplierName?: string;
  status: SupplierInvoiceStatus;
  currencyCode?: string;
  exchangeRate?: number;
  grandTotal?: number;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  shippingCost?: number;
  otherCharges?: number;
  dueDate?: string;
  notes?: string;
  created: string;
}

export interface SupplierInvoiceDetail extends SupplierInvoice {
  details: SupplierInvoiceDetailLine[];
}

export interface SupplierInvoiceDetailLine {
  id: number;
  purchaseOrderDetailId: number;
  goodsReceiptNoteDetailId?: number;
  itemId: number;
  itemName?: string;
  itemNameAr?: string;
  quantity: number;
  unitPrice: number;
  discountAmount?: number;
  taxAmount?: number;
  lineTotal?: number;
  notes?: string;
}

export const supplierInvoiceStatusLabels: Record<SupplierInvoiceStatus, string> = {
  Draft: 'مسودة',
  Submitted: 'مقدمة',
  Matched: 'مطابقة',
  PartiallyPaid: 'مدفوعة جزئياً',
  Paid: 'مدفوعة',
  Disputed: 'متنازع عليها',
  Cancelled: 'ملغاة',
};

export const supplierInvoiceStatusVariant: Record<SupplierInvoiceStatus, 'draft' | 'approved' | 'cancelled' | 'pending'> = {
  Draft: 'draft',
  Submitted: 'pending',
  Matched: 'approved',
  PartiallyPaid: 'pending',
  Paid: 'approved',
  Disputed: 'cancelled',
  Cancelled: 'cancelled',
};
