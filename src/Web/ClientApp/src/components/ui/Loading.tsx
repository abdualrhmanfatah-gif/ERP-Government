import { cn } from '@/lib/utils';

interface LoadingProps {
  text?: string;
  fullPage?: boolean;
}

export function Loading({ text = 'جاري التحميل…', fullPage = false }: LoadingProps) {
  return (
    <div
      role="status"
      aria-busy="true"
      aria-label={text}
      className={cn(
        'flex flex-col items-center justify-center gap-3',
        fullPage ? 'py-16 min-h-[60vh]' : 'py-8'
      )}
    >
      <span
        aria-hidden="true"
        className="inline-block w-8 h-8 border-[3px] border-[var(--color-border-container)] border-t-[var(--color-primary)] rounded-full animate-spin"
      />
      <span className="text-sm text-[var(--color-on-surface-variant)]">{text}</span>
    </div>
  );
}

// ── Skeleton (Protocol §35 loading states) ────────────────────────────────

interface SkeletonProps {
  className?: string;
  lines?: number;
  /** "text" = single line. "heading" = thick line. "card" = box. "avatar" = circle. "table" = table rows. */
  variant?: 'text' | 'heading' | 'card' | 'avatar' | 'table';
}

export function Skeleton({ className, lines = 1, variant = 'text' }: SkeletonProps) {
  const base = 'animate-pulse rounded-lg bg-[var(--color-surface-container-low)]';

  if (variant === 'card') {
    return (
      <div
        role="status"
        aria-busy="true"
        aria-label="جاري التحميل"
        className={cn(base, 'h-32 w-full rounded-xl', className)}
      />
    );
  }

  if (variant === 'avatar') {
    return (
      <span
        role="status"
        aria-busy="true"
        aria-label="جاري التحميل"
        className={cn(base, 'inline-block h-10 w-10 rounded-full', className)}
      />
    );
  }

  if (variant === 'table') {
    return (
      <div role="status" aria-busy="true" aria-label="جاري التحميل" className="space-y-2">
        {Array.from({ length: lines }).map((_, i) => (
          <div key={i} className={cn(base, 'h-10 w-full rounded-lg', className)} />
        ))}
      </div>
    );
  }

  return (
    <div
      role="status"
      aria-busy="true"
      aria-label="جاري التحميل"
      className="space-y-2"
    >
      {Array.from({ length: lines }).map((_, i) => (
        <div
          key={i}
          className={cn(
            base,
            variant === 'heading' ? 'h-5 w-3/4' : 'h-3 w-full',
            i === lines - 1 && 'w-2/3',
            className
          )}
        />
      ))}
    </div>
  );
}
