import { useState, useMemo } from 'react';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCostCenters } from '@/features/organization/hooks/useCostCenters';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';

export interface EntryLine {
  id: number;
  journalEntryId: number;
  sequence: number;
  accountId: number;
  description: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId: number | null;
  paymentOrderId: number | null;
  rowVersion: string;
}

interface Props {
  lines: EntryLine[];
  onAdd: (line: EntryLine) => void;
  onRemove: (index: number) => void;
}

export function EntryLinesEditor({ lines, onAdd, onRemove }: Props) {
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const { data: costCenters = [] } = useCostCenters();
  const [editing, setEditing] = useState<Partial<EntryLine> | null>(null);

  const totalDebit = useMemo(() => lines.reduce((s, l) => s + (l.debit || 0), 0), [lines]);
  const totalCredit = useMemo(() => lines.reduce((s, l) => s + (l.credit || 0), 0), [lines]);
  const accountMap = useMemo(() => new Map(accounts.map((a) => [a.id, `${a.code} - ${a.name}`])), [accounts]);
  const costCenterMap = useMemo(() => new Map(costCenters.map((c) => [c.id, `${c.code} - ${c.name}`])), [costCenters]);

  const handleAdd = () => setEditing({
    id: 0, journalEntryId: 0, sequence: lines.length + 1, accountId: 0,
    description: null, currencyId: 1, exchangeRate: 1,
    debit: 0, credit: 0, costCenterId: null,
    paymentOrderId: null, rowVersion: '',
  });

  const handleSave = () => {
    if (!editing) return;
    if (editing.debit && editing.credit) { alert('يجب أن يكون السطر مدين أو دائن فقط'); return; }
    if (!editing.debit && !editing.credit) { alert('يجب إدخال مبلغ مدين أو دائن'); return; }
    if (!editing.accountId) { alert('يجب اختيار الحساب'); return; }
    onAdd(editing as EntryLine);
    setEditing(null);
  };

  return (
    <div>
      <div className="flex justify-end mb-4">
        <Button type="button" variant="secondary" size="sm" onClick={handleAdd}>
          + إضافة سطر
        </Button>
      </div>

      {lines.length === 0 ? (
        <div className="text-center py-12 rounded-lg border-2 border-dashed border-[var(--color-outline-variant)]">
          <svg className="mx-auto h-10 w-10 mb-3 text-[var(--color-on-surface-variant)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <p className="text-sm font-medium text-[var(--color-on-surface-variant)]">لا توجد أسطر بعد</p>
        </div>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-[var(--color-outline-variant)]">
          <table className="min-w-full divide-y border-[var(--color-outline-variant)]">
            <thead>
              <tr className="bg-[var(--color-surface-container-low)]">
                <th className="px-4 py-3 text-right text-xs font-bold text-[var(--color-on-surface-variant)]">#</th>
                <th className="px-4 py-3 text-right text-xs font-bold text-[var(--color-on-surface-variant)]">الحساب</th>
                <th className="px-4 py-3 text-right text-xs font-bold text-[var(--color-on-surface-variant)]">مركز التكلفة</th>
                <th className="px-4 py-3 text-left text-xs font-bold text-[var(--color-on-surface-variant)]">مدين</th>
                <th className="px-4 py-3 text-left text-xs font-bold text-[var(--color-on-surface-variant)]">دائن</th>
                <th className="px-4 py-3 text-right text-xs font-bold text-[var(--color-on-surface-variant)]">الوصف</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y border-[var(--color-outline-variant)]">
              {lines.map((line, index) => (
                <tr key={index} className="bg-[var(--color-surface)]">
                  <td className="px-4 py-3 text-sm text-[var(--color-on-surface-variant)]">{line.sequence}</td>
                  <td className="px-4 py-3 text-sm font-bold text-[var(--color-on-surface)]">{accountMap.get(line.accountId) || line.accountId}</td>
                  <td className="px-4 py-3 text-sm text-[var(--color-on-surface-variant)]">{costCenterMap.get(line.costCenterId ?? 0) || '-'}</td>
                  <td className="px-4 py-3 text-sm text-left tabular-nums font-bold text-[var(--color-on-surface)]">{line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}</td>
                  <td className="px-4 py-3 text-sm text-left tabular-nums font-bold text-[var(--color-on-surface)]">{line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}</td>
                  <td className="px-4 py-3 text-sm text-[var(--color-on-surface-variant)]">{line.description || '-'}</td>
                  <td className="px-4 py-3 text-sm">
                    <Button type="button" variant="ghost" size="sm" onClick={() => onRemove(index)} className="text-[var(--color-error)] hover:text-[var(--color-error)]">حذف</Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editing && (
        <div className="mt-4 rounded-lg border p-5 space-y-4 bg-[var(--color-surface-container-low)] border-[var(--color-outline-variant)]">
          <h4 className="text-sm font-bold text-[var(--color-on-surface)]">سطر جديد</h4>
          <div className="grid grid-cols-4 gap-4">
            <div className="col-span-2">
              <Select
                label="الحساب"
                value={editing.accountId || ''}
                onChange={(e) => setEditing({ ...editing, accountId: Number(e.target.value) })}
                options={[
                  { value: '', label: 'اختر الحساب...' },
                  ...accounts.map((a) => ({ value: String(a.id), label: `${a.code} - ${a.name}` })),
                ]}
              />
            </div>
            <div>
              <Input
                label="مدين"
                type="number"
                value={editing.debit || ''}
                onChange={(e) => setEditing({ ...editing, debit: Number(e.target.value), credit: 0 })}
              />
            </div>
            <div>
              <Input
                label="دائن"
                type="number"
                value={editing.credit || ''}
                onChange={(e) => setEditing({ ...editing, credit: Number(e.target.value), debit: 0 })}
              />
            </div>
          </div>
          <div>
            <Input
              label="الوصف"
              type="text"
              value={editing.description || ''}
              onChange={(e) => setEditing({ ...editing, description: e.target.value })}
              placeholder="وصف السطر..."
            />
          </div>
          <div>
            <Select
              label="مركز التكلفة"
              value={editing.costCenterId || ''}
              onChange={(e) => setEditing({ ...editing, costCenterId: e.target.value ? Number(e.target.value) : null })}
              options={[
                { value: '', label: 'بدون' },
                ...costCenters.map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` })),
              ]}
            />
          </div>
          <div className="flex gap-2 pt-2">
            <Button type="button" variant="primary" size="sm" onClick={handleSave}>حفظ السطر</Button>
            <Button type="button" variant="outline" size="sm" onClick={() => setEditing(null)}>إلغاء</Button>
          </div>
        </div>
      )}

      <div className="mt-4"><BalanceIndicator totalDebit={totalDebit} totalCredit={totalCredit} /></div>
    </div>
  );
}
