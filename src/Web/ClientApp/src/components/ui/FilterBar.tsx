import { useId, type ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Filter, X } from 'lucide-react';
import { Button } from './Button';

interface FilterBarProps {
  children: ReactNode;
  label?: string;
  onClear?: () => void;
  hasFilters?: boolean;
  className?: string;
}

export function FilterBar({ children, label = 'البحث والتصفية', onClear, hasFilters, className }: FilterBarProps) {
  const groupId = useId();

  return (
    <div
      role="search"
      aria-label={label}
      className={cn(
        'flex gap-3 flex-wrap items-center p-3 bg-[var(--color-surface-container-lowest)] border border-[var(--color-border-container)] rounded-xl shadow-sm',
        className
      )}
    >
      <div className="flex items-center gap-1.5 text-[var(--color-on-surface-variant)]">
        <Filter size={14} aria-hidden="true" />
        <span className="text-xs font-medium">{label}</span>
      </div>

      <div className="contents" role="group" aria-labelledby={`${groupId}-label`}>
        {children}
      </div>

      {hasFilters && onClear && (
        <Button
          variant="ghost"
          size="sm"
          onClick={onClear}
          className="flex items-center gap-1 text-[var(--color-on-surface-variant)] hover:text-[var(--color-error)]"
        >
          <X size={14} aria-hidden="true" />
          مسح الفلاتر
        </Button>
      )}
    </div>
  );
}
