import type { FiscalYearDto, FiscalPeriodDto } from '../../../web-api-client';

export type { FiscalYearDto, FiscalPeriodDto };

export type FiscalYearStatus = 'Draft' | 'Open' | 'SoftClosed' | 'HardClosed';

export const statusColors: Record<FiscalYearStatus, string> = {
  Draft: 'bg-status-draft-bg text-status-draft-fg',
  Open: 'bg-status-active-bg text-status-active-fg',
  SoftClosed: 'bg-status-pending-bg text-status-pending-fg',
  HardClosed: 'bg-status-closed-bg text-status-closed-fg',
};

export const statusLabelsAr: Record<FiscalYearStatus, string> = {
  Draft: 'مسودة',
  Open: 'مفتوح',
  SoftClosed: 'مغلق جزئياً',
  HardClosed: 'مغلق',
};

// ─── Closing Entry Types ──────────────────────────────────────────
export type ClosingEntryStatus = 'Draft' | 'PendingApproval' | 'Approved' | 'Posted' | 'Cancelled';

export interface ClosingEntryDto {
  id: number;
  closingEntryNumber: string;
  fiscalYearId: number;
  fiscalYearName: string;
  closingDate: string;
  description?: string | null;
  status: ClosingEntryStatus;
  isReversal: boolean;
  reversalOfId?: number | null;
  reversalOfNumber?: string | null;
  moveId?: number | null;
  moveEntryNumber?: string | null;
  approvedById?: string | null;
  isActive: boolean;
  rowVersion?: string | null;
}

export const closingEntryStatusColors: Record<ClosingEntryStatus, string> = {
  Draft: 'bg-status-draft-bg text-status-draft-fg',
  PendingApproval: 'bg-status-pending-bg text-status-pending-fg',
  Approved: 'bg-status-approved-bg text-status-approved-fg',
  Posted: 'bg-status-active-bg text-status-active-fg',
  Cancelled: 'bg-status-closed-bg text-status-closed-fg',
};

export const closingEntryStatusLabelsAr: Record<ClosingEntryStatus, string> = {
  Draft: 'مسودة',
  PendingApproval: 'بانتظار الاعتماد',
  Approved: 'معتمد',
  Posted: 'قيد',
  Cancelled: 'ملغي',
};
