export type PurchaseRequestStatus = 'Draft' | 'Submitted' | 'Approved' | 'UnderProcurement' | 'Rejected' | 'Cancelled' | 'Expired';
export type PurchaseRequestPriority = 'Low' | 'Normal' | 'High' | 'Urgent';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PurchaseRequest {
  id: number;
  requestNumber: string;
  requestDate: string;
  requiredDate?: string;
  departmentId?: number;
  costCenterId?: number;
  requesterName: string;
  priority: PurchaseRequestPriority;
  status: PurchaseRequestStatus;
  totalEstimatedCost?: number;
  notes?: string;
  lineCount: number;
  created: string;
}

export interface PurchaseRequestDetail extends PurchaseRequest {
  requiredDate?: string;
  lines: PurchaseRequestDetailLine[];
}

export interface PurchaseRequestDetailLine {
  id: number;
  itemId: number;
  unitId: number;
  requestedQuantity: number;
  approvedQuantity?: number;
  unitCostEstimate?: number;
  totalCostEstimate?: number;
  notes?: string;
}

export const purchaseRequestStatusLabels: Record<PurchaseRequestStatus, string> = {
  Draft: 'مسودة',
  Submitted: 'مقدمة',
  Approved: 'معتمدة',
  UnderProcurement: 'قيد التوريد',
  Rejected: 'مرفوضة',
  Cancelled: 'ملغاة',
  Expired: 'منتهية',
};

export const purchaseRequestPriorityLabels: Record<PurchaseRequestPriority, string> = {
  Low: 'منخفضة',
  Normal: 'عادية',
  High: 'عالية',
  Urgent: 'عاجلة',
};

export const purchaseRequestStatusVariant: Record<PurchaseRequestStatus, 'draft' | 'pending' | 'approved' | 'active' | 'closed' | 'rejected' | 'cancelled'> = {
  Draft: 'draft',
  Submitted: 'pending',
  Approved: 'approved',
  UnderProcurement: 'active',
  Rejected: 'rejected',
  Cancelled: 'cancelled',
  Expired: 'closed',
};

export const purchaseRequestPriorityVariant: Record<PurchaseRequestPriority, 'default' | 'secondary' | 'danger' | 'outline' | 'warning'> = {
  Low: 'secondary',
  Normal: 'default',
  High: 'warning',
  Urgent: 'danger',
};
