import { useId, type SelectHTMLAttributes } from 'react';
import { cn } from '@/lib/utils';

interface FilterSelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

interface FilterSelectProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'onChange'> {
  label: string;
  options: FilterSelectOption[];
  value?: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
}

export function FilterSelect({
  label,
  options,
  value = '',
  onChange,
  placeholder = 'الكل',
  className,
  id: providedId,
  ...rest
}: FilterSelectProps) {
  const autoId = useId();
  const id = providedId ?? autoId;

  return (
    <div className={cn('min-w-32', className)}>
      <label htmlFor={id} className="sr-only">
        {label}
      </label>
      <select
        id={id}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className={cn(
          'w-full h-11 px-3 py-2.5 text-sm font-normal leading-normal',
          'border-2 rounded-lg bg-[var(--color-surface-container-lowest)] text-[var(--color-on-surface)]',
          'focus:outline-2 focus:outline-[var(--color-focus-ring)] focus:outline-offset-2 focus:ring-0 focus:shadow-[0_0_0_4px_var(--color-focus-halo)]',
          'disabled:opacity-50 disabled:cursor-not-allowed',
          'transition-colors duration-150',
          'appearance-none bg-no-repeat',
          'bg-[length:1rem] bg-[position:End_0.75rem_center]',
          'border-[var(--color-border-input)]'
        )}
        style={{
          backgroundImage: `url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='var(--color-on-surface-variant)' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M6 8l4 4 4-4'/%3e%3c/svg%3e")`,
        }}
        {...rest}
      >
        <option value="">{placeholder}</option>
        {options.map((opt) => (
          <option key={opt.value} value={opt.value} disabled={opt.disabled}>
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  );
}
