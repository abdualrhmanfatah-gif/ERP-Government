import { useId } from 'react';
import { Search } from 'lucide-react';
import { cn } from '@/lib/utils';

interface FilterSearchProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  label?: string;
  className?: string;
  autoFocus?: boolean;
}

export function FilterSearch({
  value,
  onChange,
  placeholder = 'بحث...',
  label = 'بحث',
  className,
  autoFocus,
}: FilterSearchProps) {
  const inputId = useId();

  return (
    <div className={cn('relative flex-1 min-w-40', className)}>
      <label htmlFor={inputId} className="sr-only">
        {label}
      </label>
      <Search
        size={14}
        className="absolute end-2.5 top-1/2 -translate-y-1/2 text-[var(--color-on-surface-variant)] pointer-events-none"
        aria-hidden="true"
      />
      <input
        id={inputId}
        type="search"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        autoFocus={autoFocus}
        className="w-full h-8 ps-2 pe-8 rounded-lg border border-[var(--color-border-input)] bg-[var(--color-surface-container-lowest)] text-xs text-[var(--color-on-surface)] placeholder:text-[var(--color-outline)] focus:outline-2 focus:outline-[var(--color-focus-ring)] focus:shadow-[0_0_0_4px_var(--color-focus-halo)] transition-colors duration-150"
      />
    </div>
  );
}
