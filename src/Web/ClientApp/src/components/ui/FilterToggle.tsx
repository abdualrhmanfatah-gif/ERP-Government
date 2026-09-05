import { useId } from 'react';
import { cn } from '@/lib/utils';

interface FilterToggleProps {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  className?: string;
}

export function FilterToggle({ label, checked, onChange, className }: FilterToggleProps) {
  const switchId = useId();

  return (
    <div className={cn('flex items-center gap-2 pb-2', className)}>
      <button
        id={switchId}
        role="switch"
        type="button"
        aria-checked={checked}
        aria-label={label}
        onClick={() => onChange(!checked)}
        className={cn(
          'relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent',
          'transition-colors duration-200 ease-in-out',
          'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus-ring',
          'disabled:cursor-not-allowed disabled:opacity-50',
          checked ? 'bg-[var(--color-primary)]' : 'bg-[var(--color-surface-container-highest)]'
        )}
      >
        <span
          className={cn(
            'pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow-lg ring-0',
            'transition duration-200 ease-in-out',
            checked ? 'translate-x-0 rtl:-translate-x-5' : 'translate-x-0'
          )}
          style={{
            transform: checked ? 'translateX(20px)' : 'translateX(0)',
          }}
        />
      </button>
      <label
        htmlFor={switchId}
        className="text-sm font-medium text-[var(--color-on-surface)] cursor-pointer select-none"
      >
        {label}
      </label>
    </div>
  );
}
