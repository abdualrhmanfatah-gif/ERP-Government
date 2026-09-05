import { cn } from '@/lib/utils';
import type { ReactNode } from 'react';

interface Column<T> {
  key: string;
  header: string;
  align?: 'start' | 'end' | 'center';
  width?: string;
  render?: (item: T, index: number) => ReactNode;
  className?: string;
}

interface ReportDataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  totalRow?: ReactNode;
  compact?: boolean;
  emptyMessage?: string;
  className?: string;
}

export function ReportDataTable<T extends Record<string, unknown>>({
  columns,
  data,
  totalRow,
  compact = false,
  emptyMessage = 'لا توجد بيانات',
  className,
}: ReportDataTableProps<T>) {
  if (data.length === 0) {
    return (
      <div className="text-center py-8 text-sm text-[var(--color-on-surface-variant)]">
        {emptyMessage}
      </div>
    );
  }

  const cellPadding = compact ? 'px-3 py-1.5' : 'px-4 py-2.5';

  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="border-b-2 border-[var(--color-outline-variant)]">
          {columns.map((col) => (
            <th
              key={col.key}
              className={cn(
                'font-semibold text-xs text-[var(--color-on-surface-variant)] uppercase tracking-wider',
                cellPadding,
                col.align === 'end' && 'text-end',
                col.align === 'center' && 'text-center',
                col.align === 'start' && 'text-start',
                !col.align && 'text-start',
                col.width && `w-[${col.width}]`,
                col.className
              )}
            >
              {col.header}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {data.map((item, index) => (
          <tr
            key={index}
            className={cn(
              'border-b border-dashed border-[var(--color-outline-variant)]',
              'hover:bg-[var(--color-surface-container)] transition-colors',
              index % 2 === 0 && 'bg-[var(--color-surface-container-lowest)]'
            )}
          >
            {columns.map((col) => (
              <td
                key={col.key}
                className={cn(
                  cellPadding,
                  'text-[var(--color-on-surface)]',
                  col.align === 'end' && 'text-end tabular-nums',
                  col.align === 'center' && 'text-center',
                  col.align === 'start' && 'text-start',
                  !col.align && 'text-start',
                  col.className
                )}
              >
                {col.render
                  ? col.render(item, index)
                  : String(item[col.key] ?? '')}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
      {totalRow && (
        <tfoot>
          <tr className="font-bold border-t-2 border-[var(--color-outline-variant)] bg-[var(--color-surface-container)]">
            {totalRow}
          </tr>
        </tfoot>
      )}
    </table>
  );
}
