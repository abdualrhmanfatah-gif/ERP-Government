export type TransferStatus = 'Draft' | 'Executed' | 'Cancelled';

export interface TransferListItem {
  id: number;
  documentNumber: string;
  assetId: number;
  assetCode: string;
  assetName: string;
  transactionDate: string;
  status: string;
  fromLocationName?: string | null;
  toLocationName?: string | null;
  fromEmployeeName?: string | null;
  toEmployeeName?: string | null;
}

export interface TransferDetail extends TransferListItem {
  currencyId: number;
  notes?: string | null;
  occurredAt: string;
  fromLocationId?: number | null;
  toLocationId?: number | null;
  fromEmployeeId?: number | null;
  toEmployeeId?: number | null;
  fromDepartmentId?: number | null;
  fromDepartmentName?: string | null;
  toDepartmentId?: number | null;
  toDepartmentName?: string | null;
  rowVersion: string;
  assetRowVersion: string;
}

export const transferStatusLabels: Record<TransferStatus, string> = {
  Draft: 'مسودة',
  Executed: 'منفذة',
  Cancelled: 'ملغاة',
};

export const transferStatusOptions = Object.entries(transferStatusLabels).map(([value, label]) => ({ value, label }));
