import { useId, type ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Switch as SwitchPrimitive } from '@base-ui/react/switch';

interface SwitchProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
  label?: ReactNode;
  id?: string;
  size?: 'sm' | 'default';
}

export function Switch({
  checked = false,
  onChange,
  disabled = false,
  label,
  id,
  size = 'default',
}: SwitchProps) {
  const autoId = useId();
  const switchId = id ?? autoId;

  return (
    <label
      htmlFor={switchId}
      className={cn(
        'inline-flex items-center gap-2 cursor-pointer min-h-11 py-1 rtl:flex-row-reverse',
        disabled && 'opacity-50 cursor-not-allowed'
      )}
    >
      <SwitchPrimitive.Root
        id={switchId}
        data-slot="switch"
        data-size={size}
        checked={checked}
        onCheckedChange={onChange}
        disabled={disabled}
        className={cn(
          'peer group/switch relative inline-flex shrink-0 items-center rounded-full border border-transparent transition-all outline-none',
          'focus-visible:ring-2 focus-visible:ring-[var(--color-focus-ring)] focus-visible:ring-offset-2',
          'data-[size=default]:h-[18.4px] data-[size=default]:w-[32px] data-[size=sm]:h-[14px] data-[size=sm]:w-[24px]',
          'data-checked:bg-[var(--color-primary)] data-unchecked:bg-[var(--color-outline-variant)]',
          'data-disabled:cursor-not-allowed data-disabled:opacity-50'
        )}
      >
        <SwitchPrimitive.Thumb
          data-slot="switch-thumb"
          className={cn(
            'pointer-events-none block rounded-full bg-[var(--color-surface-container-lowest)] shadow-sm ring-0 transition-transform',
            'group-data-[size=default]/switch:size-4 group-data-[size=sm]/switch:size-3',
            'group-data-[size=default]/switch:data-checked:translate-x-[calc(100%-2px)] group-data-[size=sm]/switch:data-checked:translate-x-[calc(100%-2px)]',
            'group-data-[size=default]/switch:data-unchecked:translate-x-0 group-data-[size=sm]/switch:data-unchecked:translate-x-0'
          )}
        />
      </SwitchPrimitive.Root>
      {label && (
        <span className="text-sm text-[var(--color-on-surface)]">{label}</span>
      )}
    </label>
  );
}
