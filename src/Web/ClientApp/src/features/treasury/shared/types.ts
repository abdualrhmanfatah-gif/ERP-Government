// Treasury shared — re-exports generated contract types + label maps.

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
  ApproveReceiptVoucherCommand,
  IApproveReceiptVoucherCommand,
  CancelReceiptVoucherCommand,
  ICancelReceiptVoucherCommand,
  RevenueClaimDto,
  IRevenueClaimDto,
  CreateRevenueClaimCommand,
  ICreateRevenueClaimCommand,
  ApproveRevenueClaimCommand,
  IApproveRevenueClaimCommand,
  CollectionOrderDto,
  ICollectionOrderDto,
  CreateCollectionOrderCommand,
  ICreateCollectionOrderCommand,
  ApproveCollectionOrderCommand,
  IApproveCollectionOrderCommand,
  DepositSlip47Dto,
  IDepositSlip47Dto,
  DepositSlip48Dto,
  IDepositSlip48Dto,
  CreateDepositSlip47Command,
  ICreateDepositSlip47Command,
  CreateDepositSlip48Command,
  ICreateDepositSlip48Command,
  ApproveDepositSlip47Command,
  IApproveDepositSlip47Command,
  ApproveDepositSlip48Command,
  IApproveDepositSlip48Command,
} from '../../../web-api-client';

export {
  PaymentMethod,
  ReceiptVoucherStatus,
  CheckStatus,
  ClaimStatus,
  CollectionOrderStatus,
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

// ─── Revenue Claim Labels ──────────────────────────────────────────────────

export const claimStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  PendingApproval: 'قيد الاعتماد',
  Open: 'مفتوحة',
  PartiallySettled: 'محصلة جزئياً',
  Settled: 'محصلة',
  WrittenOff: 'مخافة عن السداد',
};

export const claimStatusBadgeVariant: Record<string, 'secondary' | 'warning' | 'success' | 'danger' | 'primary'> = {
  Draft: 'secondary',
  PendingApproval: 'warning',
  Open: 'primary',
  PartiallySettled: 'warning',
  Settled: 'success',
  WrittenOff: 'danger',
};

// ─── Collection Order Labels ───────────────────────────────────────────────

export const collectionOrderStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  PendingApproval: 'قيد الاعتماد',
  Approved: 'معتمد',
  PartiallyCollected: 'محصّل جزئياً',
  Collected: 'محصّل',
  Cancelled: 'ملغي',
};

export const collectionOrderStatusBadgeVariant: Record<string, 'secondary' | 'warning' | 'success' | 'danger' | 'primary'> = {
  Draft: 'secondary',
  PendingApproval: 'warning',
  Approved: 'primary',
  PartiallyCollected: 'warning',
  Collected: 'success',
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
