import { StatusBadge as UIStatusBadge, type BadgeVariant } from '@/components/ui/StatusBadge';

const orderStatusVariantMap: Record<string, BadgeVariant> = {
  Draft: 'draft',
  Submitted: 'submitted',
  Approved: 'approved',
  SentToTreasury: 'sentToTreasury',
  Paid: 'paid',
  PartiallyPaid: 'partiallyPaid',
  Cancelled: 'cancelled',
  Rejected: 'rejected',
  Voided: 'voided',
};

const orderStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Submitted: 'مرسلة',
  Approved: 'موافق عليها',
  SentToTreasury: 'مرسلة للخزينة',
  Paid: 'مدفوعة',
  PartiallyPaid: 'مدفوعة جزئياً',
  Cancelled: 'ملغاة',
  Rejected: 'مرفوضة',
  Voided: 'ملغاة نهائياً',
};

const requestStatusVariantMap: Record<string, BadgeVariant> = {
  Draft: 'draft',
  PendingApproval: 'pending',
  Approved: 'approved',
  Rejected: 'rejected',
  Cancelled: 'cancelled',
  Disbursed: 'disbursed',
  Invalidated: 'cancelled',
};

const requestStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  PendingApproval: 'بانتظار الاعتماد',
  Approved: 'معتمدة',
  Rejected: 'مرفوضة',
  Cancelled: 'ملغاة',
  Disbursed: 'صرفت',
  Invalidated: 'غير صالحة',
};

const paymentStatusVariantMap: Record<string, BadgeVariant> = {
  Completed: 'approved',
  Failed: 'failed',
};

const paymentStatusLabels: Record<string, string> = {
  Completed: 'مكتمل',
  Failed: 'فشل',
};

const budgetCheckVariantMap: Record<string, BadgeVariant> = {
  Pending: 'pending',
  Passed: 'passed',
  Failed: 'failed',
  Overridden: 'overridden',
};

const budgetCheckLabels: Record<string, string> = {
  Pending: 'قيد الفحص',
  Passed: 'ناجح',
  Failed: 'فاشل',
  Overridden: 'تم التجاوز',
};

interface PaymentsStatusBadgeProps {
  status: string;
  className?: string;
  variant?: 'order' | 'request' | 'payment' | 'budgetCheck';
}

function getVariant(status: string, type: string): BadgeVariant {
  switch (type) {
    case 'order': return orderStatusVariantMap[status] ?? 'draft';
    case 'request': return requestStatusVariantMap[status] ?? 'draft';
    case 'payment': return paymentStatusVariantMap[status] ?? 'pending';
    case 'budgetCheck': return budgetCheckVariantMap[status] ?? 'pending';
    default: return 'draft';
  }
}

function getLabel(status: string, type: string): string {
  switch (type) {
    case 'order': return orderStatusLabels[status] ?? status;
    case 'request': return requestStatusLabels[status] ?? status;
    case 'payment': return paymentStatusLabels[status] ?? status;
    case 'budgetCheck': return budgetCheckLabels[status] ?? status;
    default: return status;
  }
}

export function PaymentsStatusBadge({ status, className = '', variant = 'order' }: PaymentsStatusBadgeProps) {
  return (
    <UIStatusBadge variant={getVariant(status, variant)} className={className}>
      {getLabel(status, variant)}
    </UIStatusBadge>
  );
}
