import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { disposalStatusLabels, type DisposalStatus } from './types';

const DISPOSAL_STATUS_ROLES: Record<DisposalStatus, BadgeVariant> = {
  Draft: 'draft',
  Approved: 'approved',
  Posted: 'closed',
};

export function getDisposalStatusBadge(status: string | null | undefined): { variant: BadgeVariant; label: string } {
  if (status && status in DISPOSAL_STATUS_ROLES) {
    return { variant: DISPOSAL_STATUS_ROLES[status as DisposalStatus], label: disposalStatusLabels[status as DisposalStatus] };
  }
  return { variant: 'draft', label: status ?? '—' };
}
