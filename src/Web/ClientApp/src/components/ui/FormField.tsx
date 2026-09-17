import { type ReactNode, useId } from 'react';
import { cn } from '@/lib/utils';

interface FormFieldProps {
  label: string;
  htmlFor?: string;
  error?: string;
  description?: string;
  required?: boolean;
  disabled?: boolean;
  children: ReactNode;
  className?: string;
}

export function FormField({
  label,
  htmlFor,
  error,
  description,
  required = false,
  disabled = false,
  children,
  className,
}: FormFieldProps) {
  const autoId = useId();
  const fieldId = htmlFor ?? autoId;
  const errorId = error ? `${fieldId}-error` : undefined;
  const descriptionId = description && !error ? `${fieldId}-description` : undefined;

  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <label
        htmlFor={fieldId}
        className={cn(
          'text-label-md text-[var(--color-on-surface)]',
          disabled && 'text-[var(--color-disabled-fg)] cursor-not-allowed'
        )}
      >
        {label}
        {required && (
          <span aria-hidden="true" className="text-[var(--color-error)] ms-1">
            *
          </span>
        )}
      </label>

      <div
        className={cn(
          'relative',
          error && '[&>input]:border-[var(--color-error)] [&>input]:focus-visible:ring-[var(--color-error)]',
          disabled && '[&>input]:bg-[var(--color-disabled-bg)] [&>input]:text-[var(--color-disabled-fg)] [&>input]:cursor-not-allowed'
        )}
      >
        {children}
      </div>

      {description && !error && (
        <p id={descriptionId} className="text-xs text-[var(--color-on-surface-variant)]">
          {description}
        </p>
      )}

      {error && (
        <p
          id={errorId}
          role="alert"
          className="text-xs text-[var(--color-error)]"
        >
          {error}
        </p>
      )}
    </div>
  );
}
