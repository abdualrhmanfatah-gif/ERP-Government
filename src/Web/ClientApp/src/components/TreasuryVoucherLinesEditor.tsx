// Revenue lines editor — at least one line required (FR-002).
import { Plus, Trash2 } from 'lucide-react';
import { Button, FormField, Input, Select } from '@/components/ui';
import type { LineFormRow, AccountLookupDto } from '@/features/treasury/shared/types';

interface VoucherLinesEditorProps {
  rows: LineFormRow[];
  accounts: AccountLookupDto[];
  disabled?: boolean;
  onChange: (rows: LineFormRow[]) => void;
  error?: string;
}

export function VoucherLinesEditor({ rows, accounts, disabled, onChange, error }: VoucherLinesEditorProps) {
  function updateRow(index: number, patch: Partial<LineFormRow>) {
    onChange(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));
  }

  function addRow() {
    onChange([...rows, { revenueAccountId: 0, amount: 0 }]);
  }

  function removeRow(index: number) {
    onChange(rows.filter((_, i) => i !== index));
  }

  return (
    <section aria-label="بنود الإيراد" className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-[var(--color-on-surface)]">بنود الإيراد *</h3>
        <Button type="button" variant="outline" size="sm" onClick={addRow} disabled={disabled}>
          <Plus size={14} />
          إضافة بند
        </Button>
      </div>

      {rows.map((row, index) => (
        <div
          key={index}
          className="grid grid-cols-1 md:grid-cols-3 gap-3 rounded border border-[var(--color-outline-variant)] p-3"
          data-testid={`line-row-${index}`}
        >
          <Select
            label="حساب الإيراد"
            id={`revenueAccountId-${index}`}
            value={String(row.revenueAccountId)}
            disabled={disabled}
            onChange={(e) => updateRow(index, { revenueAccountId: Number(e.target.value) })}
            options={[
              { value: '0', label: 'اختر الحساب...' },
              ...accounts.map((a) => ({ value: String(a.id), label: a.name })),
            ]}
          />
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
          <div className="flex items-end gap-3">
            <div className="flex-1">
              <FormField label="الوصف" htmlFor={`description-${index}`}>
                <Input
                  id={`description-${index}`}
                  type="text"
                  value={row.description ?? ''}
                  disabled={disabled}
                  onChange={(e) => updateRow(index, { description: e.target.value })}
                />
              </FormField>
            </div>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label={`حذف البند ${index + 1}`}
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
