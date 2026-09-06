import { useState, useEffect, useRef } from 'react';
import type { JournalEntryLineDto } from '../types';

interface ReverseDialogProps {
  entryId: number;
  entryNumber: string;
  lines: JournalEntryLineDto[];
  onConfirm: (reason: string) => void;
  onClose: () => void;
  isReversing?: boolean;
  error?: string;
}

export function ReverseDialog({
  entryNumber,
  lines,
  onConfirm,
  onClose,
  isReversing,
  error,
}: ReverseDialogProps) {
  const [reason, setReason] = useState('');
  const dialogRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    textareaRef.current?.focus();

    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [onClose]);

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) onClose();
  };

  const handleConfirm = () => {
    if (reason.trim()) {
      onConfirm(reason.trim());
    }
  };

  const inputStyle = {
    backgroundColor: 'var(--color-surface)',
    color: 'var(--color-onSurface)',
    borderColor: 'var(--color-outlineVariant)',
  };

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="عكس القيد"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      onClick={handleBackdropClick}
    >
      <div
        ref={dialogRef}
        className="bg-[var(--color-surface)] rounded-lg border p-6 w-full max-w-lg space-y-4 shadow-lg"
        style={{ borderColor: 'var(--color-outlineVariant)' }}
        onClick={(e) => e.stopPropagation()}
      >
        <h3 className="text-lg font-bold" style={{ color: 'var(--color-onSurface)' }}>
          عكس القيد
        </h3>
        <p className="text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>
          سيتم عكس القيد <strong>{entryNumber}</strong> وإنشاء قيد عكسي مرحّل فورًا.
        </p>

        <div>
          <label htmlFor="reversalReason" className="block text-sm font-bold mb-1" style={{ color: 'var(--color-onSurface)' }}>
            سبب العكس *
          </label>
          <textarea
            id="reversalReason"
            ref={textareaRef}
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            maxLength={500}
            rows={3}
            required
            aria-required="true"
            className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2"
            style={inputStyle}
            placeholder="خطأ في التسجيل"
          />
          <p className="text-xs mt-1 tabular-nums" style={{ color: 'var(--color-onSurfaceVariant)' }}>
            {reason.length}/500
          </p>
        </div>

        {lines.length > 0 && (
          <div className="rounded-lg border p-3" style={{ borderColor: 'var(--color-outlineVariant)' }}>
            <p className="text-xs font-bold mb-2" style={{ color: 'var(--color-onSurfaceVariant)' }}>
              معاينة القيد العكسي (Counter-Entry Preview)
            </p>
            <table className="w-full text-xs">
              <thead>
                <tr>
                  <th className="text-right py-1" style={{ color: 'var(--color-onSurfaceVariant)' }}>الحساب</th>
                  <th className="text-left py-1" style={{ color: 'var(--color-onSurfaceVariant)' }}>مدين</th>
                  <th className="text-left py-1" style={{ color: 'var(--color-onSurfaceVariant)' }}>دائن</th>
                </tr>
              </thead>
              <tbody>
                {lines.map((line) => (
                  <tr key={line.id} className="border-t" style={{ borderColor: 'var(--color-outlineVariant)' }}>
                    <td className="py-1" style={{ color: 'var(--color-onSurface)' }}>
                      {line.accountCode} - {line.accountName}
                    </td>
                    <td className="py-1 text-left tabular-nums" style={{ color: 'var(--color-onSurface)' }}>
                      {line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}
                    </td>
                    <td className="py-1 text-left tabular-nums" style={{ color: 'var(--color-onSurface)' }}>
                      {line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {error && (
          <p role="alert" className="text-sm" style={{ color: 'var(--color-error)' }}>
            {error}
          </p>
        )}

        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            disabled={isReversing}
            className="px-4 py-2 rounded-lg text-sm font-bold transition-colors duration-150"
            style={{
              backgroundColor: 'var(--color-surfaceContainer)',
              color: 'var(--color-onSurface)',
            }}
          >
            إلغاء
          </button>
          <button
            type="button"
            onClick={handleConfirm}
            disabled={!reason.trim() || isReversing}
            className="px-4 py-2 rounded-lg text-sm font-bold transition-colors duration-150 disabled:opacity-50"
            style={{
              backgroundColor: 'var(--color-error)',
              color: 'var(--color-onError)',
            }}
          >
            {isReversing ? 'جاري العكس...' : 'تأكيد العكس'}
          </button>
        </div>
      </div>
    </div>
  );
}
