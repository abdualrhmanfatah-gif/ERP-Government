import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';

type AlertVariant = 'warning' | 'error' | 'success' | 'info';

interface AlertProps {
  variant?: AlertVariant;
  children: ReactNode;
  className?: string;
  role?: string;
}

const variantClasses: Record<AlertVariant, string> = {
  warning:
    'border border-[var(--color-warning)] bg-[var(--color-warning-bg)] text-[var(--color-warning)]',
  error:
    'border border-[var(--color-error)] bg-[var(--color-error-container)] text-[var(--color-on-error-container)]',
  success:
    'border border-[var(--color-success)] bg-[var(--color-success-bg)] text-[var(--color-success)]',
  info:
    'border border-[var(--color-info)] bg-[var(--color-info-bg)] text-[var(--color-info)]',
};

export function Alert({ variant = 'warning', children, className, role = 'status' }: AlertProps) {
  return (
    <div
      role={role}
      className={cn('mb-3 rounded-md px-3 py-2 text-sm', variantClasses[variant], className)}
    >
      {children}
    </div>
  );
}
