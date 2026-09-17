export type RevaluationStatus = 'Draft' | 'Approved' | 'Posted';

export interface Revaluation {
  id: number;
  documentNumber: string;
  assetId: number;
  assetCode: string;
  assetName: string;
  revaluationDate: string;
  status: string;
  revaluationAmount: number;
  type: string;
  created: string;
}

export interface RevaluationDetail extends Revaluation {
  previousValue: number;
  newValue: number;
  revaluationMethod: string;
  notes?: string;
  rowVersion: string;
}

export const revaluationStatusLabels: Record<RevaluationStatus, string> = {
  Draft: 'مسودة',
  Approved: 'معتمدة',
  Posted: 'مُقيّدة',
};

export const revaluationStatusOptions = Object.entries(revaluationStatusLabels).map(([value, label]) => ({ value, label }));
