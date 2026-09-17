export type CountStatus = 'Draft' | 'InProgress' | 'Completed' | 'Reviewed';

export interface AssetCount {
  id: number;
  documentNumber: string;
  countDate: string;
  countType: string;
  status: string;
  resolvedScopeLabel: string;
  totalAssets: number;
  foundCount: number;
  notFoundCount: number;
  notExaminedCount: number;
  created: string;
}

export interface CountDetail extends AssetCount {
  locationId?: number;
  departmentId?: number;
  notes?: string;
  rowVersion: string;
  lines: CountLine[];
}

export interface CountLine {
  id: number;
  assetId: number;
  assetCode: string;
  assetName: string;
  isFound: number;
  physicalLocation?: string;
  physicalCustodian?: string;
  physicalStatus?: string;
  discrepancyNotes?: string;
  rowVersion: string;
}

export const countStatusLabels: Record<CountStatus, string> = {
  Draft: 'مسودة',
  InProgress: 'قيد التنفيذ',
  Completed: 'مكتملة',
  Reviewed: 'تمت المراجعة',
};

export const isFoundLabels: Record<number, string> = {
  0: 'لم يفحص',
  1: 'موجود',
  2: 'غير موجود',
};

export const countStatusOptions = Object.entries(countStatusLabels).map(([value, label]) => ({ value, label }));
