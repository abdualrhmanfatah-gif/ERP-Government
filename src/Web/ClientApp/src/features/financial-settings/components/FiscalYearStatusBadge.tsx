import { StatusBadge } from '@/components/ui';
import { fiscalYearStatusLabels, FiscalYearStatus } from '../shared/types';

const statusVariantMap: Record<string, 'draft' | 'active' | 'closed' | 'posted'> = {
  Draft: 'draft',
  Open: 'active',
  SoftClosed: 'closed',
  HardClosed: 'posted',
};

interface FiscalYearStatusBadgeProps {
  status: string;
  size?: 'sm' | 'md';
}

export function FiscalYearStatusBadge({ status, size = 'md' }: FiscalYearStatusBadgeProps) {
  const variant = statusVariantMap[status] ?? 'draft';
  const label = fiscalYearStatusLabels[FiscalYearStatus[status as keyof typeof FiscalYearStatus] as FiscalYearStatus] ?? status;

  return (
    <StatusBadge variant={variant} size={size}>
      {label}
    </StatusBadge>
  );
}
