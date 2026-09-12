import { useId } from 'react';
import { cn } from '@/lib/utils';

interface FilterDateProps {
  label: string;
  value?: string;
  onChange: (value: string) => void;
  min?: string;
  max?: string;
  className?: string;
}

export function FilterDate({ label, value = '', onChange, min, max, className }: FilterDateProps) {
  const inputId = useId();

  return (
    <div className={cn('min-w-32', className)}>
      <label htmlFor={inputId} className="sr-only">
        {label}
      </label>
      <input
        id={inputId}
        type="date"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        min={min}
        max={max}
        className="w-full h-11 px-3 py-2.5 text-sm font-normal leading-normal border-2 rounded-lg bg-[var(--color-surface-container-lowest)] text-[var(--color-on-surface)] focus:outline-2 focus:outline-[var(--color-focus-ring)] focus:outline-offset-2 focus:ring-0 focus:shadow-[0_0_0_4px_var(--color-focus-halo)] transition-colors duration-150 border-[var(--color-border-input)]"
      />
    </div>
  );
}
