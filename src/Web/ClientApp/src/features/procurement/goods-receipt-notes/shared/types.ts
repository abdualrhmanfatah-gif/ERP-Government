export type GRNStatus = 'Draft' | 'Confirmed' | 'Rejected';

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface GRN {
  id: number;
  grnNumber: string;
  grnDate: string;
  purchaseOrderId: number;
  purchaseOrderNumber?: string;
  supplierPartyId?: number;
  supplierName?: string;
  warehouseId: number;
  warehouseName?: string;
  status: GRNStatus;
  notes?: string;
  created: string;
}

export interface GRNDetail extends GRN {
  locationId: number;
  locationName?: string;
  receivedBy?: number;
  receivedByName?: string;
  details: GRNDetailLine[];
}

export interface GRNDetailLine {
  id: number;
  purchaseOrderDetailId: number;
  itemId: number;
  itemName?: string;
  unitId: number;
  unitName?: string;
  orderedQuantity: number;
  receivedQuantity: number;
  acceptedQuantity?: number;
  rejectedQuantity?: number;
  remainingQuantity: number;
  unitCost?: number;
  totalCost?: number;
  batchNumber?: string;
  expiryDate?: string;
  notes?: string;
}

export const grnStatusLabels: Record<GRNStatus, string> = {
  Draft: 'مسودة',
  Confirmed: 'مؤكدة',
  Rejected: 'مرفوضة',
};

export const grnStatusVariant: Record<GRNStatus, 'draft' | 'approved' | 'cancelled'> = {
  Draft: 'draft',
  Confirmed: 'approved',
  Rejected: 'cancelled',
};
