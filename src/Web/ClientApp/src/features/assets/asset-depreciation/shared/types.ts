export type DepreciationRunStatus = 'Draft' | 'Posting' | 'Posted';

export interface DepreciationRun {
  id: number;
  runNumber: string;
  fiscalYear: string;
  periodNumber: number;
  depreciationDate: string;
  assetsCount: number;
  totalDepreciation: number;
  status: DepreciationRunStatus;
  journalEntryId?: number;
  missedPeriodsPolicy?: string;
}

export interface DepreciationScheduleDetail {
  id: number;
  assetId: number;
  assetCode: string;
  assetName: string;
  method: string;
  rate: number;
  periodNumber?: number;
  totalPeriods?: number;
  depreciationBase: number;
  residualValue: number;
  openingBookValue: number;
  openingAccumulatedDepreciation: number;
  amount: number;
  closingAccumulatedDepreciation: number;
  closingBookValue: number;
}

export interface DepreciationRunDetail extends DepreciationRun {
  fiscalYearId: number;
  fiscalPeriodId: number;
  notes?: string;
  rowVersion: string;
  scheduleLines: DepreciationScheduleDetail[];
}

export interface RunDepreciationRequest {
  fiscalYearId: number;
  fiscalPeriodId: number;
  depreciationDate: string;
  missedPeriodsPolicy?: string;
  notes?: string;
}

export const depreciationStatusLabels: Record<DepreciationRunStatus, string> = {
  Draft: 'مسودة',
  Posting: 'قيد الترحيل',
  Posted: 'تم الترحيل',
};

export const depreciationStatusOptions = Object.entries(depreciationStatusLabels)
  .map(([value, label]) => ({ value, label }));

export interface AssetDepreciationSchedule {
  id: number;
  depreciationDate: string;
  fiscalYear: string;
  periodNumber: number;
  amount: number;
  accumulatedDepreciation: number;
  status: DepreciationRunStatus;
  journalEntryId?: number;
}

export interface PreviewDepreciationRequest {
  fiscalYearId: number;
  fiscalPeriodId: number;
  depreciationDate: string;
  missedPeriodsPolicy?: string;
}

export interface PreviewScheduleLine {
  assetId: number;
  assetCode: string;
  assetName?: string;
  days: number;
  rate: number;
  amount: number;
  originalValue: number;
  accumulatedDepreciation: number;
  openingBookValue: number;
  closingBookValue: number;
  assetStatus: string;
}

export interface PreviewDepreciationResult {
  totalAssets: number;
  totalDepreciation: number;
  lines: PreviewScheduleLine[];
}

export const missedPeriodsPolicyOptions = [
  { value: 'CurrentPeriodOnly', label: 'الفترة الحالية فقط' },
  { value: 'CatchUp', label: 'تعويض الفترات الفائتة' },
];
