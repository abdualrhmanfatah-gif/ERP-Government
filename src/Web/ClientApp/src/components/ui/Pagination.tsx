import { ChevronRight, ChevronLeft } from 'lucide-react';
import { cn } from '@/lib/utils';

interface PaginationProps {
  page: number;
  total: number;
  pageSize: number;
  onChange: (page: number) => void;
}

export function Pagination({ page, total, pageSize, onChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  if (totalPages <= 1) return null;

  return (
    <nav aria-label="التنقل بين الصفحات" className="flex items-center gap-1 justify-center">
      <button
        type="button"
        aria-label="الصفحة السابقة"
        disabled={page <= 1}
        onClick={() => onChange(page - 1)}
        className={cn(
          'inline-flex items-center justify-center w-11 h-11 border border-[var(--color-border-input)] rounded-lg bg-transparent p-0 focus-visible:ring-2 focus-visible:ring-[var(--color-focus-ring)]',
          page <= 1
            ? 'text-[var(--color-disabled-fg)] cursor-not-allowed bg-[var(--color-disabled-bg)]'
            : 'text-[var(--color-primary)] cursor-pointer hover:bg-[var(--color-surface-container-low)]'
        )}
      >
        <ChevronRight size={16} />
      </button>

      {Array.from({ length: totalPages }, (_, i) => i + 1)
        .filter((p) => p === 1 || p === totalPages || Math.abs(p - page) <= 1)
        .reduce<(number | '...')[]>((acc, p, i, arr) => {
          if (i > 0 && p - (arr[i - 1] as number) > 1) acc.push('...');
          acc.push(p);
          return acc;
        }, [])
        .map((item, i) =>
          item === '...' ? (
            <span key={`e-${i}`} aria-hidden="true" className="px-1 text-[var(--color-outline)]">…</span>
          ) : (
            <button
              key={item}
              type="button"
              aria-label={`صفحة ${item}`}
              aria-current={item === page ? 'page' : undefined}
              onClick={() => onChange(item as number)}
              className={cn(
                'inline-flex items-center justify-center min-w-11 h-11 rounded-lg px-2 text-[0.8125rem] cursor-pointer transition-colors duration-150 focus-visible:ring-2 focus-visible:ring-[var(--color-focus-ring)]',
                item === page
                  ? 'border border-[var(--color-primary)] bg-[var(--color-primary)] text-[var(--color-on-primary)] font-semibold'
                  : 'border border-transparent text-[var(--color-primary)] hover:bg-[var(--color-surface-container-low)]'
              )}
            >
              {item}
            </button>
          )
        )}

      <button
        type="button"
        aria-label="الصفحة التالية"
        disabled={page >= totalPages}
        onClick={() => onChange(page + 1)}
        className={cn(
          'inline-flex items-center justify-center w-11 h-11 border border-[var(--color-border-input)] rounded-lg bg-transparent p-0 focus-visible:ring-2 focus-visible:ring-[var(--color-focus-ring)]',
          page >= totalPages
            ? 'text-[var(--color-disabled-fg)] cursor-not-allowed bg-[var(--color-disabled-bg)]'
            : 'text-[var(--color-primary)] cursor-pointer hover:bg-[var(--color-surface-container-low)]'
        )}
      >
        <ChevronLeft size={16} />
      </button>

      <span className="ms-3 text-xs text-[var(--color-outline)]">
        صفحة {page} من {totalPages}
      </span>
    </nav>
  );
}
