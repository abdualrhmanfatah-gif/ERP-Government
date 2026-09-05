import {
  closingEntryStatusColors,
  closingEntryStatusLabelsAr,
} from '../types';
import { Button } from '@/components/ui/Button';
import type { ClosingEntryDto, ClosingEntryStatus } from '../types';

interface ClosingEntriesListProps {
  entries: ClosingEntryDto[];
  isLoading?: boolean;
  onSelect?: (entry: ClosingEntryDto) => void;
}

export function ClosingEntriesList({ entries, isLoading, onSelect }: ClosingEntriesListProps) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8 text-sm text-[var(--color-on-surface-variant)]">
        جاري التحميل...
      </div>
    );
  }

  if (entries.length === 0) {
    return (
      <div className="flex items-center justify-center py-8 text-sm text-[var(--color-on-surface-variant)]">
        لا توجد قيود إغلاق
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-[var(--color-border-container)] text-start text-xs font-medium uppercase tracking-wider text-[var(--color-on-surface-variant)]">
            <th className="px-4 py-3">رقم القيد</th>
            <th className="px-4 py-3">التاريخ</th>
            <th className="px-4 py-3">الحالة</th>
            <th className="px-4 py-3">قيد إرتجاعي</th>
            <th className="px-4 py-3">القيد المحاسبي</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {entries.map((entry) => (
            <tr key={entry.id} className="hover:bg-[var(--color-surface-container-low)]">
              <td className="whitespace-nowrap px-4 py-3 font-medium">
                {onSelect ? (
                  <Button variant="ghost" size="sm" onClick={() => onSelect(entry)}>
                    {entry.closingEntryNumber}
                  </Button>
                ) : (
                  entry.closingEntryNumber
                )}
              </td>
              <td className="whitespace-nowrap px-4 py-3 tabular-nums">
                {new Date(entry.closingDate).toLocaleDateString('ar-EG')}
              </td>
              <td className="px-4 py-3">
                <span
                  className={`inline-flex items-center rounded-full px-3 py-0.5 text-xs font-medium ${
                    closingEntryStatusColors[entry.status as ClosingEntryStatus] ?? ''
                  }`}
                >
                  {closingEntryStatusLabelsAr[entry.status as ClosingEntryStatus] ?? entry.status}
                </span>
              </td>
              <td className="px-4 py-3 text-center">
                {entry.isReversal ? (
                  <span className="text-[var(--color-warning)]" title={entry.reversalOfNumber ?? ''}>
                    نعم
                  </span>
                ) : (
                  <span className="text-[var(--color-on-surface-variant)]">—</span>
                )}
              </td>
              <td className="whitespace-nowrap px-4 py-3 text-[var(--color-on-surface-variant)]">
                {entry.moveEntryNumber ?? '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
