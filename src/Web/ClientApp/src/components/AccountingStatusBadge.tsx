import { StatusBadge as UIStatusBadge, type BadgeVariant } from '@/components/ui/StatusBadge';
import { EntryStatus } from '@/features/accounting/types';

const statusVariantMap: Record<EntryStatus, BadgeVariant> = {
  [EntryStatus.Draft]: 'draft',
  [EntryStatus.Submitted]: 'pending',
  [EntryStatus.Approved]: 'approved',
  [EntryStatus.Posted]: 'posted',
  [EntryStatus.Reversed]: 'reversed',
  [EntryStatus.Cancelled]: 'cancelled',
};

const statusLabels: Record<EntryStatus, string> = {
  [EntryStatus.Draft]: 'مسودة',
  [EntryStatus.Submitted]: 'مرسلة للمراجعة',
  [EntryStatus.Approved]: 'موافق عليها',
  [EntryStatus.Posted]: 'مرحل',
  [EntryStatus.Reversed]: 'معكوسة',
  [EntryStatus.Cancelled]: 'ملغاة',
};

interface StatusBadgeProps {
  status: EntryStatus;
  className?: string;
}

export function StatusBadge({ status, className = '' }: StatusBadgeProps) {
  const variant = statusVariantMap[status] ?? 'draft';
  const label = statusLabels[status] ?? 'مسودة';

  return (
    <UIStatusBadge variant={variant} className={className}>
      {label}
    </UIStatusBadge>
  );
}
