import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { revaluationStatusLabels, type RevaluationStatus } from './types';

const REVALUATION_STATUS_ROLES: Record<RevaluationStatus, BadgeVariant> = {
  Draft: 'draft',
  Approved: 'approved',
  Posted: 'closed',
};

export function getRevaluationStatusBadge(status: string | null | undefined): { variant: BadgeVariant; label: string } {
  if (status && status in REVALUATION_STATUS_ROLES) {
    return { variant: REVALUATION_STATUS_ROLES[status as RevaluationStatus], label: revaluationStatusLabels[status as RevaluationStatus] };
  }
  return { variant: 'draft', label: status ?? '—' };
}
