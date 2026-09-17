export type ImpairmentStatus = 'Draft' | 'Approved' | 'Posted';

export interface Impairment {
  id: number;
  documentNumber: string;
  assetId: number;
  assetCode: string;
  assetName: string;
  impairmentDate: string;
  status: string;
  impairmentAmount: number;
  isReversal: boolean;
  created: string;
}

export interface ImpairmentDetail extends Impairment {
  previousValue: number;
  newValue: number;
  reversalOfTransactionId?: number;
  reversalReason?: string;
  notes?: string;
  rowVersion: string;
}

export const impairmentStatusLabels: Record<ImpairmentStatus, string> = {
  Draft: 'مسودة',
  Approved: 'معتمدة',
  Posted: 'مُقيّدة',
};

export const impairmentStatusOptions = Object.entries(impairmentStatusLabels).map(([value, label]) => ({ value, label }));
