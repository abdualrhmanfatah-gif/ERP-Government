// Treasury shared — re-exports generated contract types + label maps.
// Contract source: specs/031-receipt-vouchers/contracts/api.md (binding, literal).

export type {
  ReceiptVoucherDto,
  IReceiptVoucherDto,
  ReceiptVoucherLineDto,
  IReceiptVoucherLineDto,
  CheckDto,
  ICheckDto,
  CreateCheckDto,
  ICreateCheckDto,
  CreateReceiptVoucherCommand,
  ICreateReceiptVoucherCommand,
  CreateReceiptVoucherLineDto,
  ICreateReceiptVoucherLineDto,
  SubmitReceiptVoucherCommand,
  ISubmitReceiptVoucherCommand,
  ApproveReceiptVoucherCommand,
  IApproveReceiptVoucherCommand,
  CancelReceiptVoucherCommand,
  ICancelReceiptVoucherCommand,
} from '../../../web-api-client';

export {
  PaymentMethod,
  ReceiptVoucherStatus,
  CheckStatus,
} from '../../../web-api-client';

export const paymentMethodLabels: Record<string, string> = {
  '1': 'نقدي',
  '2': 'شيكات',
};

export const voucherStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  PendingReview: 'قيد المراجعة',
  Approved: 'معتمد',
  Cancelled: 'ملغي',
};

export const voucherStatusBadgeVariant: Record<string, 'secondary' | 'warning' | 'success' | 'danger'> = {
  Draft: 'secondary',
  PendingReview: 'warning',
  Approved: 'success',
  Cancelled: 'danger',
};

// ─── Deposit Slip Labels ────────────────────────────────────────────────────

export const depositSlipStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Approved: 'معتمدة',
};

export const depositSlipFormTypeLabels: Record<string, string> = {
  Form47: 'نقدية (47)',
  Form48: 'شيكات (48)',
};

// ─── Check Labels ───────────────────────────────────────────────────────────

export const checkStatusLabels: Record<string, string> = {
  UnderCollection: 'تحت التحصيل',
  Cleared: 'محصل',
  Bounced: 'مرتجع',
};

export interface AccountLookupDto {
  id: number;
  code: string;
  name: string;
}

export interface ReceiptVoucherFilters {
  partyId?: number;
  paymentMethod?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

export interface CheckFormRow {
  bankName: string;
  checkNumber: string;
  checkDate: string;
  amount: number;
}

export interface LineFormRow {
  revenueAccountId: number;
  amount: number;
  description?: string;
}
