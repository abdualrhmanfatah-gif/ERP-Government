import { useId, type ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface SwitchProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
  label?: ReactNode;
  id?: string;
}

export function Switch({
  checked = false,
  onChange,
  disabled = false,
  label,
  id,
}: SwitchProps) {
  const autoId = useId();
  const switchId = id ?? autoId;

  return (
    <label
      htmlFor={switchId}
      className={cn(
        'inline-flex items-center gap-2 cursor-pointer min-h-11 py-1',
        disabled && 'opacity-50 cursor-not-allowed'
      )}
    >
      <button
        id={switchId}
        type="button"
        role="switch"
        aria-checked={checked}
        disabled={disabled}
        onClick={() => onChange?.(!checked)}
        className={cn(
          'relative inline-flex h-6 w-11 shrink-0 items-center rounded-full border border-transparent transition-colors duration-150 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-focus-ring)] focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
          checked ? 'bg-[var(--color-primary)]' : 'bg-[var(--color-outline-variant)]'
        )}
      >
        <span
          className={cn(
            'pointer-events-none block h-5 w-5 rounded-full bg-[var(--color-surface-container-lowest)] shadow-sm transition-transform duration-150 rtl:translate-x-reverse',
            checked ? 'translate-x-5' : 'translate-x-0'
          )}
        />
      </button>
      {label && (
        <span className="text-sm text-[var(--color-on-surface)]">{label}</span>
      )}
    </label>
  );
}
