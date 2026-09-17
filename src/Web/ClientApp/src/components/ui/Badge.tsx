import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';

type BadgeVariant = 'default' | 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'error' | 'outline';

interface BadgeProps {
  variant?: BadgeVariant;
  children: ReactNode;
  className?: string;
}

const variantClasses: Record<BadgeVariant, string> = {
  default: 'bg-[var(--color-surface-container)] text-[var(--color-on-surface-variant)]',
  primary: 'bg-[var(--color-primary)] text-[var(--color-on-primary)]',
  secondary: 'bg-[var(--color-secondary)] text-[var(--color-on-secondary)]',
  success: 'bg-status-approved-bg text-status-approved-fg',
  warning: 'bg-status-pending-bg text-status-pending-fg',
  danger: 'bg-[var(--color-error-container)] text-[var(--color-on-error-container)]',
  error: 'bg-[var(--color-error)] text-[var(--color-on-error)]',
  outline: 'border border-[var(--color-outline)] text-[var(--color-on-surface)]',
};

export function Badge({ variant = 'default', children, className }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium whitespace-nowrap',
        variantClasses[variant],
        className,
      )}
    >
      {children}
    </span>
  );
}
