import { useState, useRef } from 'react';
import type { JournalEntryLineDto } from '@/features/accounting/types';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { FormField } from '@/components/ui/FormField';

interface ReverseDialogProps {
  open: boolean;
  entryId: number;
  entryNumber: string;
  lines: JournalEntryLineDto[];
  onConfirm: (reason: string) => void;
  onClose: () => void;
  isReversing?: boolean;
  error?: string;
}

function ReverseEntryPreview({ lines }: { lines: JournalEntryLineDto[] }) {
  return (
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
  );
}

export function ReverseDialog({
  open,
  entryNumber,
  lines,
  onConfirm,
  onClose,
  isReversing,
  error,
}: ReverseDialogProps) {
  const [reason, setReason] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const handleConfirm = () => {
    if (reason.trim()) {
      onConfirm(reason.trim());
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="عكس القيد"
      footer={
        <>
          <Button variant="ghost" onClick={onClose} disabled={isReversing}>إلغاء</Button>
          <Button variant="destructive" onClick={handleConfirm} loading={isReversing} disabled={!reason.trim()}>
            {isReversing ? 'جاري العكس...' : 'تأكيد العكس'}
          </Button>
        </>
      }
    >
      <p className="text-sm text-[var(--color-on-surface-variant)]">
        سيتم عكس القيد <strong>{entryNumber}</strong> وإنشاء قيد عكسي مرحّل فورًا.
      </p>

      <FormField label="سبب العكس" required error={error}>
        <textarea
          ref={textareaRef}
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          maxLength={500}
          rows={3}
          aria-required="true"
          className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2"
          style={{
            backgroundColor: 'var(--color-surface)',
            color: 'var(--color-onSurface)',
            borderColor: 'var(--color-outlineVariant)',
          }}
          placeholder="خطأ في التسجيل"
        />
      </FormField>
      <p className="text-xs tabular-nums text-[var(--color-on-surface-variant)]">{reason.length}/500</p>

      {lines.length > 0 && <ReverseEntryPreview lines={lines} />}
    </Dialog>
  );
}
