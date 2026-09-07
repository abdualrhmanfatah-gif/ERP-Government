// Checks section — rendered/required only when payment method = Check (FR-003).
import { Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui';
import type { CheckFormRow } from '@/features/treasury/shared/types';

interface ChecksSectionProps {
  visible: boolean;
  rows: CheckFormRow[];
  disabled?: boolean;
  onChange: (rows: CheckFormRow[]) => void;
  error?: string;
}

export function ChecksSection({ visible, rows, disabled, onChange, error }: ChecksSectionProps) {
  if (!visible) return null;

  function updateRow(index: number, patch: Partial<CheckFormRow>) {
    onChange(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));
  }

  function addRow() {
    onChange([...rows, { bankName: '', checkNumber: '', checkDate: '', amount: 0 }]);
  }

  function removeRow(index: number) {
    onChange(rows.filter((_, i) => i !== index));
  }

  return (
    <section aria-label="قسم الشيكات" className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-[var(--color-on-surface)]">الشيكات *</h3>
        <Button type="button" variant="outline" size="sm" onClick={addRow} disabled={disabled}>
          <Plus size={14} />
          إضافة شيك
        </Button>
      </div>

      {rows.length === 0 && <p className="text-xs text-[var(--color-on-surface-variant)]">أضف شيكاً واحداً على الأقل.</p>}

      {rows.map((row, index) => (
        <div
          key={index}
          className="grid grid-cols-1 md:grid-cols-5 gap-3 rounded border border-[var(--color-outline-variant)] p-3"
          data-testid={`check-row-${index}`}
        >
          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">البنك *</label>
            <input
              type="text"
              value={row.bankName}
              disabled={disabled}
              onChange={(e) => updateRow(index, { bankName: e.target.value })}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            />
          </div>
          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم الشيك *</label>
            <input
              type="text"
              value={row.checkNumber}
              disabled={disabled}
              onChange={(e) => updateRow(index, { checkNumber: e.target.value })}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            />
          </div>
          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">تاريخ الشيك *</label>
            <input
              type="date"
              value={row.checkDate}
              disabled={disabled}
              onChange={(e) => updateRow(index, { checkDate: e.target.value })}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            />
          </div>
          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">المبلغ *</label>
            <input
              type="number"
              min={0}
              step="0.01"
              value={row.amount}
              disabled={disabled}
              onChange={(e) => updateRow(index, { amount: Number(e.target.value) })}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm tabular-nums"
            />
          </div>
          <div className="flex items-end">
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label={`حذف الشيك ${index + 1}`}
              disabled={disabled}
              onClick={() => removeRow(index)}
            >
              <Trash2 size={14} />
            </Button>
          </div>
        </div>
      ))}

      {error && <p className="text-xs text-[var(--color-error)]">{error}</p>}
    </section>
  );
}
