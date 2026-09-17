import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { depreciationStatusLabels, type DepreciationRunStatus } from './types';

const DEPRECIATION_STATUS_ROLES: Record<DepreciationRunStatus, BadgeVariant> = {
  Draft: 'draft',
  Posting: 'pending',
  Posted: 'closed',
};

export function getDepreciationRunStatusBadge(status: string | null | undefined): { variant: BadgeVariant; label: string } {
  if (status && status in DEPRECIATION_STATUS_ROLES) {
    return {
      variant: DEPRECIATION_STATUS_ROLES[status as DepreciationRunStatus],
      label: depreciationStatusLabels[status as DepreciationRunStatus],
    };
  }
  return { variant: 'draft', label: status ?? '—' };
}