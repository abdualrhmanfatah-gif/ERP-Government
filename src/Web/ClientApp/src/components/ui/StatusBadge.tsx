import type { ReactNode } from 'react';
import { TriangleAlert } from 'lucide-react';
import { cn } from '@/lib/utils';

export type BadgeVariant = 'draft' | 'pending' | 'approved' | 'active' | 'closed' | 'inactive' | 'posted' | 'reversed' | 'cancelled' | 'locked' | 'overBudget' | 'unbalanced' | 'submitted' | 'sentToTreasury' | 'paid' | 'partiallyPaid' | 'rejected' | 'voided' | 'disbursed' | 'failed' | 'warning' | 'overridden' | 'passed';
type BadgeSize = 'sm' | 'md';

interface StatusBadgeProps {
  variant: BadgeVariant;
  size?: BadgeSize;
  icon?: ReactNode;
  children: ReactNode;
  className?: string;
}

const variantClasses: Record<BadgeVariant, string> = {
  draft: 'bg-status-draft-bg text-status-draft-fg',
  pending: 'bg-status-pending-bg text-status-pending-fg',
  approved: 'bg-status-approved-bg text-status-approved-fg',
  active: 'bg-status-active-bg text-status-active-fg',
  closed: 'bg-status-closed-bg text-status-closed-fg',
  inactive: 'bg-status-inactive-bg text-status-inactive-fg',
  posted: 'bg-status-posted-bg text-status-posted-fg',
  reversed: 'bg-status-reversed-bg text-status-reversed-fg',
  cancelled: 'bg-status-closed-bg text-status-closed-fg',
  locked: 'bg-status-locked-bg text-status-locked-fg',
  overBudget: 'bg-status-overBudget-bg text-status-overBudget-fg',
  unbalanced: 'border-2 border-status-unbalanced-border bg-transparent text-status-unbalanced-fg',
  submitted: 'bg-status-pending-bg text-status-pending-fg',
  sentToTreasury: 'bg-status-pending-bg text-status-pending-fg',
  paid: 'bg-status-approved-bg text-status-approved-fg',
  partiallyPaid: 'bg-status-active-bg text-status-active-fg',
  rejected: 'bg-status-reversed-bg text-status-reversed-fg',
  voided: 'bg-status-closed-bg text-status-closed-fg',
  disbursed: 'bg-status-approved-bg text-status-approved-fg',
  failed: 'bg-status-reversed-bg text-status-reversed-fg',
  warning: 'bg-status-overBudget-bg text-status-overBudget-fg',
  overridden: 'bg-status-overBudget-bg text-status-overBudget-fg',
  passed: 'bg-status-active-bg text-status-active-fg',
};

const sizeClasses: Record<BadgeSize, string> = {
  sm: 'px-2 py-0.5 text-[0.6875rem]',
  md: 'px-3 py-0.5 text-xs',
};

// Validation-type aliases share a base color pair with lifecycle states, so they
// carry a non-color cue by default (contracts/status-semantics.md §2, FR-003).
const defaultIcons: Partial<Record<BadgeVariant, ReactNode>> = {
  warning: <TriangleAlert className="size-3.5" aria-hidden="true" />,
  overBudget: <TriangleAlert className="size-3.5" aria-hidden="true" />,
  overridden: <TriangleAlert className="size-3.5" aria-hidden="true" />,
  unbalanced: <TriangleAlert className="size-3.5" aria-hidden="true" />,
};

const statusLabels: Record<BadgeVariant, string> = {
  draft: 'مسودة',
  pending: 'قيد المراجعة',
  approved: 'موافق',
  active: 'نشط',
  closed: 'مغلق',
  inactive: 'غير نشط',
  posted: 'مرحل',
  reversed: 'معكوس',
  cancelled: 'ملغى',
  locked: 'مقفل',
  overBudget: 'يتجاوز الميزانية',
  unbalanced: 'غير متوازن',
  submitted: 'مرسلة',
  sentToTreasury: 'مرسلة للخزينة',
  paid: 'مدفوعة',
  partiallyPaid: 'مدفوعة جزئياً',
  rejected: 'مرفوضة',
  voided: 'ملغاة نهائياً',
  disbursed: 'صرفت',
  failed: 'فاشلة',
  warning: 'تحذير',
  overridden: 'تم التجاوز',
  passed: 'ناجح',
};

export function StatusBadge({
  variant,
  size = 'md',
  icon,
  children,
  className,
}: StatusBadgeProps) {
  const cue = icon ?? defaultIcons[variant];

  return (
    <span
      role="status"
      aria-label={statusLabels[variant]}
      className={cn(
        'inline-flex items-center gap-1 rounded-full font-medium leading-normal whitespace-nowrap',
        variantClasses[variant],
        sizeClasses[size],
        className
      )}
    >
      {cue ? <span aria-hidden="true">{cue}</span> : null}
      {children}
    </span>
  );
}
