// Payments Shared Types — Cross-entity types for all payments sub-features
// Status labels shared across disbursement-requests, payments, payment-orders, bank-accounts

import {
  DisbursementRequestStatus,
  PaymentStatus,
  PaymentMethod,
  PaymentOrderStatus,
} from '../../../web-api-client';

export {
  DisbursementRequestStatus,
  PaymentStatus,
  PaymentMethod,
  PaymentOrderStatus,
};

// ─── Disbursement Request Status ────────────────────────────────────────────

export const disbursementRequestStatusLabels: Record<DisbursementRequestStatus, string> = {
  [DisbursementRequestStatus.Draft]: 'مسودة',
  [DisbursementRequestStatus.PendingApproval]: 'بانتظار الاعتماد',
  [DisbursementRequestStatus.Approved]: 'موافق عليها',
  [DisbursementRequestStatus.Rejected]: 'مرفوضة',
  [DisbursementRequestStatus.Cancelled]: 'ملغاة',
  [DisbursementRequestStatus.Disbursed]: 'مصروفة',
  [DisbursementRequestStatus.Invalidated]: 'ملغاة نهائياً',
};

export const disbursementRequestStatusOptions = Object.entries(disbursementRequestStatusLabels).map(
  ([value, label]) => ({ value, label }),
);

// ─── Payment Status ─────────────────────────────────────────────────────────

export const paymentStatusLabels: Record<PaymentStatus, string> = {
  [PaymentStatus.Completed]: 'مكتملة',
  [PaymentStatus.Failed]: 'فاشلة',
};

export const paymentMethodLabels: Record<PaymentMethod, string> = {
  [PaymentMethod.Cash]: 'نقدي',
  [PaymentMethod.Check]: 'شيك',
};

// ─── Payment Order Status ───────────────────────────────────────────────────

export const paymentOrderStatusLabels: Record<PaymentOrderStatus, string> = {
  [PaymentOrderStatus.Draft]: 'مسودة',
  [PaymentOrderStatus.Submitted]: 'مرسلة',
  [PaymentOrderStatus.Approved]: 'موافق عليها',
  [PaymentOrderStatus.SentToTreasury]: 'مرسلة للخزينة',
  [PaymentOrderStatus.Paid]: 'مدفوعة',
  [PaymentOrderStatus.Cancelled]: 'ملغاة',
  [PaymentOrderStatus.Rejected]: 'مرفوضة',
  [PaymentOrderStatus.Voided]: 'ملغاة نهائياً',
};

export const paymentOrderStatusOptions = Object.entries(paymentOrderStatusLabels).map(
  ([value, label]) => ({ value, label }),
);

// ─── Common Labels ──────────────────────────────────────────────────────────

export const booleanLabels = {
  true: 'نعم',
  false: 'لا',
} as const;
