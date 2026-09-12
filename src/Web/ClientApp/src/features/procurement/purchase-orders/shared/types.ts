export type PurchaseOrderStatus = 'Draft' | 'Submitted' | 'Approved' | 'Issued' | 'PartiallyReceived' | 'Received' | 'Closed' | 'Cancelled';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PurchaseOrder {
  id: number;
  poNumber?: string;
  purchaseOrderNumber?: string;
  poDate?: string;
  purchaseRequestId?: number;
  quotationId?: number;
  supplierPartyId: number;
  supplierName?: string;
  status: PurchaseOrderStatus;
  grandTotal?: number;
  expectedDeliveryDate?: string;
  created?: string;
}

export interface PurchaseOrderDetail extends PurchaseOrder {
  paymentTerms?: string;
  deliveryTerms?: string;
  expectedDeliveryDate?: string;
  notes?: string;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  shippingCost?: number;
  otherCharges?: number;
  lines: PurchaseOrderDetailLine[];
}

export interface PurchaseOrderDetailLine {
  id: number;
  purchaseRequestDetailId: number;
  quotationDetailId?: number;
  itemId: number;
  unitId: number;
  orderedQuantity: number;
  receivedQuantity: number;
  remainingQuantity: number;
  unitPrice: number;
  discountPercent?: number;
  discountAmount?: number;
  netUnitPrice?: number;
  taxPercent?: number;
  taxAmount?: number;
  lineTotal?: number;
  lineTotalWithTax?: number;
  notes?: string;
}

export const purchaseOrderStatusLabels: Record<PurchaseOrderStatus, string> = {
  Draft: 'مسودة',
  Submitted: 'مقدمة',
  Approved: 'معتمدة',
  Issued: 'صادرة',
  PartiallyReceived: 'مستلمة جزئياً',
  Received: 'مستلمة',
  Closed: 'مغلقة',
  Cancelled: 'ملغاة',
};

export const purchaseOrderStatusVariant: Record<PurchaseOrderStatus, 'draft' | 'pending' | 'approved' | 'active' | 'closed' | 'cancelled'> = {
  Draft: 'draft',
  Submitted: 'pending',
  Approved: 'approved',
  Issued: 'active',
  PartiallyReceived: 'pending',
  Received: 'approved',
  Closed: 'closed',
  Cancelled: 'cancelled',
};
