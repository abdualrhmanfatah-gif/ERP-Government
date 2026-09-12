// Checks section — rendered/required only when payment method = Check (FR-003).
import { Plus, Trash2 } from 'lucide-react';
import { Button, FormField, Input } from '@/components/ui';
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
          <FormField label="البنك" htmlFor={`bankName-${index}`} required>
            <Input
              id={`bankName-${index}`}
              type="text"
              value={row.bankName}
              disabled={disabled}
              onChange={(e) => updateRow(index, { bankName: e.target.value })}
            />
          </FormField>
          <FormField label="رقم الشيك" htmlFor={`checkNumber-${index}`} required>
            <Input
              id={`checkNumber-${index}`}
              type="text"
              value={row.checkNumber}
              disabled={disabled}
              onChange={(e) => updateRow(index, { checkNumber: e.target.value })}
            />
          </FormField>
          <FormField label="تاريخ الشيك" htmlFor={`checkDate-${index}`} required>
            <Input
              id={`checkDate-${index}`}
              type="date"
              value={row.checkDate}
              disabled={disabled}
              onChange={(e) => updateRow(index, { checkDate: e.target.value })}
            />
          </FormField>
          <FormField label="المبلغ" htmlFor={`amount-${index}`} required>
            <Input
              id={`amount-${index}`}
              type="number"
              min={0}
              step="0.01"
              value={row.amount}
              disabled={disabled}
              onChange={(e) => updateRow(index, { amount: Number(e.target.value) })}
              className="tabular-nums"
            />
          </FormField>
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
