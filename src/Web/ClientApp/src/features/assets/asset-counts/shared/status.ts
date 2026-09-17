import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { countStatusLabels, type CountStatus } from './types';

const COUNT_STATUS_ROLES: Record<CountStatus, BadgeVariant> = {
  Draft: 'draft',
  InProgress: 'active',
  Completed: 'pending',
  Reviewed: 'closed',
};

export function getCountStatusBadge(status: string | null | undefined): { variant: BadgeVariant; label: string } {
  if (status && status in COUNT_STATUS_ROLES) {
    return { variant: COUNT_STATUS_ROLES[status as CountStatus], label: countStatusLabels[status as CountStatus] };
  }
  return { variant: 'draft', label: status ?? '—' };
}
