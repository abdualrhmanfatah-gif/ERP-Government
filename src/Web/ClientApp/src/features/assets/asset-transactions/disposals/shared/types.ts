export type DisposalStatus = 'Draft' | 'Approved' | 'Posted';

export interface Disposal {
  id: number;
  documentNumber: string;
  assetId: number;
  assetCode: string;
  assetName: string;
  disposalDate: string;
  status: string;
  disposalMethod: string;
  netProceeds?: number;
  gainOrLoss?: number;
  created: string;
}

export interface DisposalDetail extends Disposal {
  carryingAmount: number;
  notes?: string;
  rowVersion: string;
}

export const disposalStatusLabels: Record<DisposalStatus, string> = {
  Draft: 'مسودة',
  Approved: 'معتمدة',
  Posted: 'مُقيّدة',
};

export const disposalMethodLabels: Record<string, string> = {
  Sale: 'بيع',
  Donation: 'هدية',
  Scrap: 'خردة',
  Other: 'أخرى',
};

export const disposalStatusOptions = Object.entries(disposalStatusLabels).map(([value, label]) => ({ value, label }));
