export type QuotationStatus = 'Draft' | 'Submitted' | 'UnderEvaluation' | 'Evaluated' | 'Selected' | 'Awarded' | 'Rejected' | 'Expired';

export interface Quotation {
  id: number;
  quotationNumber: string;
  supplierPartyId: number;
  quotationDate: string;
  validUntil?: string;
  status: QuotationStatus;
  grandTotal?: number;
  created: string;
}

export interface QuotationDetail extends Quotation {
  currencyCode?: string;
  exchangeRate?: number;
  shippingCost?: number;
  otherCharges?: number;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  paymentTerms?: string;
  deliveryTerms?: string;
  leadTimeDays?: number;
  warrantyPeriodMonths?: number;
  technicalScore?: number;
  financialScore?: number;
  selectionReason?: string;
  rejectionReason?: string;
  notes?: string;
  lines: QuotationDetailLine[];
}

export interface QuotationDetailLine {
  id: number;
  purchaseRequestDetailId: number;
  itemId: number;
  unitId: number;
  quantity: number;
  unitPrice?: number;
  discountPercent?: number;
  discountAmount?: number;
  netUnitPrice?: number;
  taxPercent?: number;
  taxAmount?: number;
  lineTotal?: number;
  lineTotalWithTax?: number;
  notes?: string;
}

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const quotationStatusLabels: Record<QuotationStatus, string> = {
  Draft: 'مسودة',
  Submitted: 'مقدمة',
  UnderEvaluation: 'قيد التقييم',
  Evaluated: 'تم التقييم',
  Selected: 'مختارة',
  Awarded: 'مرساة',
  Rejected: 'مرفوضة',
  Expired: 'منتهية',
};

export const quotationStatusVariants: Record<QuotationStatus, string> = {
  Draft: 'secondary',
  Submitted: 'default',
  UnderEvaluation: 'default',
  Evaluated: 'outline',
  Selected: 'default',
  Awarded: 'default',
  Rejected: 'destructive',
  Expired: 'destructive',
};
