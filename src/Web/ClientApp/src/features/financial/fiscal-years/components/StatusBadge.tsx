import { StatusBadge as SharedStatusBadge } from '@/components/ui/StatusBadge';
import type { FiscalYearStatus } from '../types';
import { statusLabelsAr } from '../types';

interface StatusBadgeProps {
  status: FiscalYearStatus;
}

const badgeVariantMap: Record<FiscalYearStatus, 'draft' | 'pending' | 'active' | 'closed'> = {
  Draft: 'draft',
  Open: 'active',
  SoftClosed: 'pending',
  HardClosed: 'closed',
};

export function StatusBadge({ status }: StatusBadgeProps) {
  return <SharedStatusBadge variant={badgeVariantMap[status]}>{statusLabelsAr[status]}</SharedStatusBadge>;
}
