import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { transferStatusLabels, type TransferStatus } from './types';

const TRANSFER_STATUS_ROLES: Record<TransferStatus, BadgeVariant> = {
  Draft: 'draft',
  Executed: 'closed',
  Cancelled: 'closed',
};

export function getTransferStatusBadge(status: string | null | undefined): { variant: BadgeVariant; label: string } {
  if (status && status in TRANSFER_STATUS_ROLES) {
    return { variant: TRANSFER_STATUS_ROLES[status as TransferStatus], label: transferStatusLabels[status as TransferStatus] };
  }
  return { variant: 'draft', label: status ?? '—' };
}
