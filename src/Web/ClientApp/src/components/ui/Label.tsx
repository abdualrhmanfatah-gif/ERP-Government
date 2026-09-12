import { type LabelHTMLAttributes } from 'react';
import { cn } from '@/lib/utils';

interface LabelProps extends LabelHTMLAttributes<HTMLLabelElement> {
  required?: boolean;
}

export function Label({ className, required, children, ...rest }: LabelProps) {
  return (
    <label
      className={cn(
        'text-label-md text-[var(--color-on-surface)]',
        className,
      )}
      {...rest}
    >
      {children}
      {required && (
        <span aria-hidden="true" className="text-[var(--color-error)] ms-1">
          *
        </span>
      )}
    </label>
  );
}
