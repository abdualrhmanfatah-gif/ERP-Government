import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import {
  closingEntryStatusColors,
  closingEntryStatusLabelsAr,
} from '../types';
import type { ClosingEntryDto, ClosingEntryStatus } from '../types';

interface ClosingEntryDetailProps {
  entry: ClosingEntryDto;
  onApprove?: (id: number) => void;
  onReverse?: (id: number, reason?: string) => void;
  isApproving?: boolean;
  isReversing?: boolean;
}

export function ClosingEntryDetail({
  entry,
  onApprove,
  onReverse,
  isApproving,
  isReversing,
}: ClosingEntryDetailProps) {
  const [showReverseDialog, setShowReverseDialog] = useState(false);
  const [reverseReason, setReverseReason] = useState('');

  const formatDate = (d: string | Date | undefined) =>
    d ? new Date(d).toLocaleDateString('ar-EG') : '—';

  const canApprove = entry.status === 'Draft' && onApprove;
  const canReverse = entry.status === 'Posted' && !entry.isReversal && onReverse;

  return (
    <div className="space-y-4">
      <div className="rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] p-4">
        <h3 className="mb-3 text-sm font-semibold text-[var(--color-on-surface)]">معلومات القيد</h3>
        <dl className="grid grid-cols-2 gap-3 text-sm">
          <div>
            <dt className="text-[var(--color-on-surface-variant)]">رقم القيد</dt>
            <dd className="mt-0.5 font-medium">{entry.closingEntryNumber}</dd>
          </div>
          <div>
            <dt className="text-[var(--color-on-surface-variant)]">السنة المالية</dt>
            <dd className="mt-0.5 font-medium">{entry.fiscalYearName}</dd>
          </div>
          <div>
            <dt className="text-[var(--color-on-surface-variant)]">التاريخ</dt>
            <dd className="mt-0.5 font-medium tabular-nums">{formatDate(entry.closingDate)}</dd>
          </div>
          <div>
            <dt className="text-[var(--color-on-surface-variant)]">الحالة</dt>
            <dd className="mt-0.5">
              <span
                className={`inline-flex items-center rounded-full px-3 py-0.5 text-xs font-medium ${
                  closingEntryStatusColors[entry.status as ClosingEntryStatus] ?? ''
                }`}
              >
                {closingEntryStatusLabelsAr[entry.status as ClosingEntryStatus] ?? entry.status}
              </span>
            </dd>
          </div>
          {entry.description && (
            <div className="col-span-2">
              <dt className="text-[var(--color-on-surface-variant)]">الوصف</dt>
              <dd className="mt-0.5">{entry.description}</dd>
            </div>
          )}
          {entry.isReversal && entry.reversalOfNumber && (
            <div>
              <dt className="text-[var(--color-on-surface-variant)]">إرتجاع من</dt>
              <dd className="mt-0.5 text-[var(--color-warning)]">{entry.reversalOfNumber}</dd>
            </div>
          )}
          {entry.moveEntryNumber && (
            <div>
              <dt className="text-[var(--color-on-surface-variant)]">القيد المحاسبي</dt>
              <dd className="mt-0.5">{entry.moveEntryNumber}</dd>
            </div>
          )}
        </dl>
      </div>

      <div className="flex gap-2">
        {canApprove && (
          <Button
            variant="primary"
            size="sm"
            onClick={() => onApprove(entry.id)}
            disabled={isApproving}
          >
            {isApproving ? 'جاري الاعتماد...' : 'اعتماد'}
          </Button>
        )}
        {canReverse && (
          <Button
            variant="destructive"
            size="sm"
            onClick={() => setShowReverseDialog(true)}
            disabled={isReversing}
          >
            {isReversing ? 'جاري الإرتجاع...' : 'إرتجاع'}
          </Button>
        )}
      </div>

      {showReverseDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="mx-4 w-full max-w-md rounded-lg bg-[var(--color-surface-container-lowest)] p-6 shadow-xl">
            <h4 className="mb-2 text-lg font-semibold">تأكيد الإرتجاع</h4>
            <p className="mb-4 text-sm text-[var(--color-on-surface-variant)]">
              سيتم إنشاء قيد إرتجاعي واستعادة السنة المالية. هل أنت متأكد؟
            </p>
            <div className="mb-4">
              <label htmlFor="reverse-reason" className="mb-1 block text-sm text-[var(--color-on-surface)]">سبب الإرتجاع (اختياري)</label>
              <textarea
                id="reverse-reason"
                value={reverseReason}
                onChange={(e) => setReverseReason(e.target.value)}
                className="w-full rounded-lg border border-[var(--color-border-input)] px-3 py-2 text-sm focus:border-[var(--color-focus-ring)] focus:outline-none"
                rows={3}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setShowReverseDialog(false);
                  setReverseReason('');
                }}
              >
                إلغاء
              </Button>
              <Button
                variant="destructive"
                size="sm"
                onClick={() => {
                  onReverse?.(entry.id, reverseReason || undefined);
                  setShowReverseDialog(false);
                  setReverseReason('');
                }}
              >
                تأكيد الإرتجاع
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
