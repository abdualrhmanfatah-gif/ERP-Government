import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { impairmentStatusLabels, type ImpairmentStatus } from './types';

const IMPAIRMENT_STATUS_ROLES: Record<ImpairmentStatus, BadgeVariant> = {
  Draft: 'draft',
  Approved: 'approved',
  Posted: 'closed',
};

export function getImpairmentStatusBadge(status: string | null | undefined): { variant: BadgeVariant; label: string } {
  if (status && status in IMPAIRMENT_STATUS_ROLES) {
    return { variant: IMPAIRMENT_STATUS_ROLES[status as ImpairmentStatus], label: impairmentStatusLabels[status as ImpairmentStatus] };
  }
  return { variant: 'draft', label: status ?? '—' };
}
