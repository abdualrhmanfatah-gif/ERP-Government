import { cn } from '@/lib/utils';
import type { ReactNode } from 'react';

const trendColors = {
  positive: 'text-[var(--color-success)]',
  negative: 'text-[var(--color-error)]',
  neutral: 'text-[var(--color-on-surface)]',
} as const;

interface ReportSummaryCardProps {
  label: string;
  value: string | number;
  icon?: ReactNode;
  trend?: keyof typeof trendColors;
  subtitle?: string;
  className?: string;
}

export function ReportSummaryCard({ 
  label, 
  value, 
  icon, 
  trend = 'neutral', 
  subtitle, 
  className 
}: ReportSummaryCardProps) {
  
 
  const formattedValue = typeof value === 'number' 
    ? new Intl.NumberFormat('en-US').format(value)
    : value;

  return (
    <div
      className={cn(
        'relative flex items-start gap-4 p-4 rounded-lg',
        'bg-[var(--color-surface-container-lowest)] border border-[var(--color-border-container)]',
    
        'border-s-[4px] border-s-[var(--color-secondary)]',
        'transition-all duration-200 hover:shadow-md',
        className
      )}
    >
      {icon && (
        <div className="flex-shrink-0 p-2.5 rounded-lg bg-[var(--color-secondary-container)] text-[var(--color-on-secondary-container)]">
          {icon}
        </div>
      )}
      
      <div className="flex-1 min-w-0">
     
        <p 
          className="text-sm font-medium text-[var(--color-on-surface-variant)] truncate"
          title={label}
        >
          {label}
        </p>
        
        <p className={cn('text-2xl font-bold tabular-nums mt-1.5 leading-tight truncate', trendColors[trend])} title={String(formattedValue)}>
          {formattedValue}
        </p>
        
        {subtitle && (
          <p 
            className="text-xs text-[var(--color-on-surface-variant)] mt-1.5 truncate"
            title={subtitle}
          >
            {subtitle}
          </p>
        )}
      </div>
    </div>
  );
}