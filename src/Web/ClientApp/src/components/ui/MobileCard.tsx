import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface MobileCardProps {
  children: ReactNode;
  className?: string;
  onClick?: () => void;
  onKeyDown?: (e: React.KeyboardEvent) => void;
  tabIndex?: number;
  role?: string;
  'aria-selected'?: boolean;
}

export function MobileCard({
  children,
  className,
  onClick,
  onKeyDown,
  tabIndex,
  role,
  'aria-selected': ariaSelected,
}: MobileCardProps) {
  return (
    <div
      className={cn(
        'bg-[var(--color-surface-container-lowest)] rounded-lg border border-[var(--color-container-border)] p-4',
        onClick && 'cursor-pointer hover:bg-[var(--color-surface-container-low)] transition-colors duration-150',
        ariaSelected && 'bg-[color-mix(in_srgb,var(--color-primary-container)_10%,transparent)] border-e-3 border-e-[var(--color-primary)]',
        className
      )}
      onClick={onClick}
      onKeyDown={onKeyDown}
      tabIndex={tabIndex}
      role={role}
      aria-selected={ariaSelected}
    >
      {children}
    </div>
  );
}

interface MobileCardFieldProps {
  label: string;
  value: ReactNode;
  className?: string;
}

export function MobileCardField({ label, value, className }: MobileCardFieldProps) {
  return (
    <div className={cn('flex flex-col gap-0.5', className)}>
      <span className="text-xs text-[var(--color-on-surface-variant)]">{label}</span>
      <span className="text-sm text-[var(--color-on-surface)]">{value ?? '—'}</span>
    </div>
  );
}