// Re-export from payments-level shared types (single source of truth)
export {
  DisbursementRequestStatus,
  disbursementRequestStatusLabels,
  disbursementRequestStatusOptions,
} from '../../shared/types';

// Entity-specific types remain here
export interface ApprovalStep {
  step: number;
  approverUserId: number;
  approverName: string;
  role: string;
  decision: string;
  decisionAt: string | null;
  approvedAmount: number | null;
  issuingAuthorityName: string | null;
  issuingAuthorityCapacity: string | null;
  reason: string | null;
}

export interface DisbursementRequestListItem {
  id: number;
  requestNumber: string;
  requestedById: number;
  requestedByName: string;
  beneficiaryName: string;
  requestedAmount: number;
  currencyId: number;
  purpose: string;
  financialYearId: number;
  requestDate: string;
  status: string;
  notes: string | null;
  paymentDate: string | null;
  paymentOrderId: number | null;
  paymentOrderNumber: string | null;
  accrualJournalEntryId: number | null;
  accrualEntryNumber: string | null;
  approvals: ApprovalStep[];
  rowVersion: string ;
}

export interface DisbursementRequestDetail {
  id: number;
  requestNumber: string;
  requestedById: number;
  requestedByName: string;
  beneficiaryName: string;
  beneficiaryPartyId: number | null;
  requestedAmount: number;
  currencyId: number;
  purpose: string;
  financialYearId: number;
  requestDate: string;
  status: string;
  notes: string | null;
  paymentDate: string | null;
  paymentOrderId: number | null;
  paymentOrderNumber: string | null;
  accrualJournalEntryId: number | null;
  accrualEntryNumber: string | null;
  approvals: ApprovalStep[];
  statusLog: StatusLogEntry[];
  rowVersion: string ;
}

export interface StatusLogEntry {
  action: string;
  performedBy: string;
  performedAt: string;
  reason: string | null;
}

export const approvalDecisionLabels: Record<string, string> = {
  Approved: 'موافق',
  Rejected: 'مرفوض',
  Cancelled: 'ملغى',
};

export type SignatureProgress = 'لم يبدأ' | 'توقيع واحد من اثنين' | 'اكتمل التوقيعان';

export function getSignatureProgress(approvals: ApprovalStep[]): SignatureProgress {
  const validApprovals = approvals.filter(a => a.decision === 'Approved');
  if (validApprovals.length === 0) return 'لم يبدأ';
  if (validApprovals.length === 1) return 'توقيع واحد من اثنين';
  return 'اكتمل التوقيعان';
}

export function getFinalApprovedAmount(approvals: ApprovalStep[]): number | null {
  const validApprovals = approvals.filter(a => a.decision === 'Approved' && a.approvedAmount != null);
  if (validApprovals.length < 2) return null;
  return validApprovals[validApprovals.length - 1].approvedAmount;
}
