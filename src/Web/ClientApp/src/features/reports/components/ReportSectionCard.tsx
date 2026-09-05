import { cn } from '@/lib/utils';
import type { ReactNode } from 'react';

interface ReportSectionCardProps {
  title: string;
  titleEn?: string;
  total?: number;
  currency?: string;
  children: ReactNode;
  variant?: 'default' | 'highlighted' | 'compact';
  className?: string;
}

export function ReportSectionCard({
  title,
  titleEn,
  total,
  currency = 'YER',
  children,
  variant = 'default',
  className,
}: ReportSectionCardProps) {
  const variantStyles = {
    default: 'bg-[var(--color-surface-container-lowest)] border-[var(--color-border-container)]',
    highlighted: 'bg-[var(--color-surface-container-low)] border-[var(--color-secondary)]',
    compact: 'bg-[var(--color-surface-container-lowest)] border-[var(--color-border-container)]',
  };

  return (
    <div
      className={cn(
        'rounded-lg border overflow-hidden',
        variantStyles[variant],
        className
      )}
    >
      <div className="flex items-center justify-between px-4 py-3 border-b border-[var(--color-border-container)] bg-[var(--color-surface-container)]">
        <div>
          <h3 className="text-sm font-bold text-[var(--color-on-surface)]">{title}</h3>
          {titleEn && (
            <p className="text-xs text-[var(--color-on-surface-variant)] mt-0.5">{titleEn}</p>
          )}
        </div>
        {total !== undefined && (
          <div className="text-start">
            <p className="text-xs text-[var(--color-on-surface-variant)]">الإجمالي</p>
            <p className="text-lg font-bold tabular-nums text-[var(--color-on-surface)]">
              {total.toLocaleString()}
            </p>
          </div>
        )}
      </div>
      <div className="overflow-x-auto">{children}</div>
    </div>
  );
}
