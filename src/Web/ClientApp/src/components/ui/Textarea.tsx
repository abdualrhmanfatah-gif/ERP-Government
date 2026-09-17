import { useId, type TextareaHTMLAttributes } from 'react';
import { cn } from '@/lib/utils';

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
}

export function Textarea({ label, error, className, id, required, ...rest }: TextareaProps) {
  const autoId = useId();
  const textareaId = id ?? autoId;
  const errorId = `${textareaId}-error`;

  return (
    <div className="flex flex-col gap-1">
      {label && (
        <label
          htmlFor={textareaId}
          className="text-label-md text-[var(--color-on-surface)]"
        >
          {label}
          {required && (
            <span aria-hidden="true" className="text-[var(--color-error)] ms-1">*</span>
          )}
        </label>
      )}
      <textarea
        id={textareaId}
        aria-invalid={!!error || undefined}
        aria-describedby={error ? errorId : undefined}
        required={required}
        className={cn(
          'w-full rounded-lg border-2 border-[var(--color-input-border)] bg-[var(--color-surface-container-lowest)] px-3 py-2.5 text-sm text-[var(--color-on-surface)] placeholder:text-[var(--color-outline)] transition-colors duration-150 focus:outline-2 focus:outline-[var(--color-focus-ring)] focus:outline-offset-2 focus:ring-0 focus:shadow-[0_0_0_4px_var(--color-focus-halo)] disabled:bg-[var(--color-disabled-bg)] disabled:text-[var(--color-disabled-fg)] disabled:cursor-not-allowed min-h-[80px] resize-y',
          error && 'border-[var(--color-error)] focus:outline-[var(--color-error)] focus:shadow-[0_0_0_4px_var(--color-error-container)]',
          className
        )}
        {...rest}
      />
      {error && (
        <p id={errorId} role="alert" className="text-xs text-[var(--color-error)]">
          {error}
        </p>
      )}
    </div>
  );
}
