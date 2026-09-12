import { useId, type SelectHTMLAttributes } from 'react';
import { cn } from '@/lib/utils';

interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  options: SelectOption[];
  error?: string;
  loading?: boolean;
  disabled?: boolean;
}

export function Select({
  label,
  options,
  error,
  loading,
  disabled,
  id: providedId,
  className,
  ...rest
}: SelectProps) {
  const autoId = useId();
  const id = providedId ?? autoId;
  const errorId = `${id}-error`;

  return (
    <div className="mb-0">
      <label
        htmlFor={id}
        className="mb-1 block text-label-md text-[var(--color-on-surface)]"
      >
        {label}
      </label>
      <select
        id={id}
        aria-invalid={!!error || undefined}
        aria-describedby={error ? errorId : undefined}
        disabled={disabled || loading}
        className={cn(
          'w-full px-3 py-2.5 text-sm font-normal leading-normal h-11',
          'border rounded-lg bg-[var(--color-surface-container-lowest)] text-[var(--color-on-surface)]',
          'focus:outline-2 focus:outline-[var(--color-focus-ring)] focus:outline-offset-2 focus:ring-0 focus:shadow-[0_0_0_4px_var(--color-focus-halo)]',
          'disabled:opacity-50 disabled:cursor-not-allowed',
          'transition-colors duration-150',
          'appearance-none bg-no-repeat',
          'bg-[length:0.875rem] bg-[position:End_0.5rem_center]',
          error
            ? 'border-2 border-[var(--color-error)] focus:outline-[var(--color-error)] focus:shadow-[0_0_0_4px_var(--color-error-container)] focus:border-[var(--color-error)]'
            : 'border-2 border-[var(--color-border-input)]',
          className
        )}
        style={{
          backgroundImage: `url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='var(--color-on-surface-variant)' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M6 8l4 4 4-4'/%3e%3c/svg%3e")`,
        }}
        {...rest}
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value} disabled={opt.disabled}>
            {opt.label}
          </option>
        ))}
      </select>
      {error ? (
        <small
          id={errorId}
          role="alert"
          className="mt-1 text-xs text-[var(--color-error)] block"
        >
          {error}
        </small>
      ) : null}
    </div>
  );
}
