import { useState } from 'react';
import { AlertTriangle, ChevronDown, ChevronUp } from 'lucide-react';
import { Button } from './Button';

interface ErrorStateProps {
  message?: string;
  onRetry?: () => void;
  details?: string;
}

export function ErrorState({
  message = 'حدث خطأ غير متوقع',
  onRetry,
  details,
}: ErrorStateProps) {
  const [showDetails, setShowDetails] = useState(false);

  return (
    <div
      role="alert"
      aria-live="assertive"
      className="flex flex-col items-center justify-center gap-3 py-12 px-4 text-center"
    >
      <span aria-hidden="true" className="text-[var(--color-error)]">
        <AlertTriangle size={32} strokeWidth={1.5} />
      </span>
      <span className="text-sm text-[var(--color-error)]">{message}</span>
      {onRetry ? (
        <Button variant="secondary" size="sm" onClick={onRetry}>
          إعادة المحاولة
        </Button>
      ) : null}
      {details ? (
        <div className="w-full max-w-3xl">
          <button
            type="button"
            onClick={() => setShowDetails(!showDetails)}
            aria-expanded={showDetails}
            className="inline-flex items-center gap-1 bg-transparent border-none text-[var(--color-on-surface-variant)] text-xs cursor-pointer p-2 min-h-11 hover:text-[var(--color-on-surface)] focus-visible:ring-2 focus-visible:ring-[var(--color-focus-ring)] rounded-lg"
          >
            {showDetails ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
            {showDetails ? 'إخفاء التفاصيل' : 'عرض التفاصيل'}
          </button>
          {showDetails ? (
            <pre className="mt-2 p-3 bg-[var(--color-surface-container)] rounded-lg text-xs text-[var(--color-on-surface-variant)] text-start overflow-auto break-words whitespace-pre-wrap">
              {details}
            </pre>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}
