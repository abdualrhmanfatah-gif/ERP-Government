import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';

type BadgeVariant = 'draft' | 'pending' | 'approved' | 'active' | 'closed' | 'posted' | 'reversed' | 'cancelled' | 'locked' | 'overBudget' | 'unbalanced';
type BadgeSize = 'sm' | 'md';

interface StatusBadgeProps {
  variant: BadgeVariant;
  size?: BadgeSize;
  icon?: ReactNode;
  children: ReactNode;
}

const variantClasses: Record<BadgeVariant, string> = {
  draft: 'bg-status-draft-bg text-status-draft-fg',
  pending: 'bg-status-pending-bg text-status-pending-fg',
  approved: 'bg-status-approved-bg text-status-approved-fg',
  active: 'bg-status-active-bg text-status-active-fg',
  closed: 'bg-status-closed-bg text-status-closed-fg',
  posted: 'bg-status-posted-bg text-status-posted-fg',
  reversed: 'bg-status-reversed-bg text-status-reversed-fg',
  cancelled: 'bg-status-cancelled-bg text-status-cancelled-fg',
  locked: 'bg-status-locked-bg text-status-locked-fg',
  overBudget: 'bg-status-overBudget-bg text-status-overBudget-fg',
  unbalanced: 'border-2 border-status-unbalanced-border bg-transparent text-status-unbalanced-fg',
};

const sizeClasses: Record<BadgeSize, string> = {
  sm: 'px-2 py-0.5 text-[0.6875rem]',
  md: 'px-3 py-0.5 text-xs',
};

const statusLabels: Record<BadgeVariant, string> = {
  draft: 'مسودة',
  pending: 'قيد المراجعة',
  approved: 'موافق',
  active: 'نشط',
  closed: 'مغلق',
  posted: 'مرحل',
  reversed: 'معكوس',
  cancelled: 'ملغى',
  locked: 'مقفل',
  overBudget: 'يتجاوز الميزانية',
  unbalanced: 'غير متوازن',
};

export function StatusBadge({
  variant,
  size = 'md',
  icon,
  children,
}: StatusBadgeProps) {
  return (
    <span
      role="status"
      aria-label={statusLabels[variant]}
      className={cn(
        'inline-flex items-center gap-1 rounded-full font-medium leading-normal whitespace-nowrap',
        variantClasses[variant],
        sizeClasses[size]
      )}
    >
      {icon ? <span aria-hidden="true">{icon}</span> : null}
      {children}
    </span>
  );
}
