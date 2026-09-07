import { useState, useMemo } from 'react';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCostCenters } from '@/features/organization/hooks/useCostCenters';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';

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

  const inputCls = 'w-full px-3 py-2 rounded-lg border text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-focus-ring)] focus:border-[var(--color-focus-ring)] transition-colors duration-200';
  const labelCls = 'block text-sm font-bold mb-1.5';

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
        <button type="button" onClick={handleAdd}
          className="px-4 py-2 text-sm font-bold rounded-lg transition-all duration-200 cursor-pointer hover:shadow-sm"
          style={{ backgroundColor: 'var(--color-secondary-container)', color: 'var(--color-on-secondary-container)' }}>
          + إضافة سطر
        </button>
      </div>

      {lines.length === 0 ? (
        <div className="text-center py-12 rounded-lg border-2 border-dashed" style={{ borderColor: 'var(--color-outlineVariant)' }}>
          <svg className="mx-auto h-10 w-10 mb-3" style={{ color: 'var(--color-onSurfaceVariant)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <p className="text-sm font-medium" style={{ color: 'var(--color-onSurfaceVariant)' }}>لا توجد أسطر بعد</p>
        </div>
      ) : (
        <div className="overflow-x-auto rounded-lg border" style={{ borderColor: 'var(--color-outlineVariant)' }}>
          <table className="min-w-full divide-y" style={{ borderColor: 'var(--color-outlineVariant)' }}>
            <thead>
              <tr style={{ backgroundColor: 'var(--color-surfaceContainerLow)' }}>
                <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>#</th>
                <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>الحساب</th>
                <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>مركز التكلفة</th>
                <th className="px-4 py-3 text-left text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>مدين</th>
                <th className="px-4 py-3 text-left text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>دائن</th>
                <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>الوصف</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y" style={{ borderColor: 'var(--color-outlineVariant)' }}>
              {lines.map((line, index) => (
                <tr key={index} style={{ backgroundColor: 'var(--color-surface)' }}>
                  <td className="px-4 py-3 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>{line.sequence}</td>
                  <td className="px-4 py-3 text-sm font-bold" style={{ color: 'var(--color-onSurface)' }}>{accountMap.get(line.accountId) || line.accountId}</td>
                  <td className="px-4 py-3 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>{costCenterMap.get(line.costCenterId ?? 0) || '-'}</td>
                  <td className="px-4 py-3 text-sm text-left tabular-nums font-bold" style={{ color: 'var(--color-onSurface)' }}>{line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}</td>
                  <td className="px-4 py-3 text-sm text-left tabular-nums font-bold" style={{ color: 'var(--color-onSurface)' }}>{line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}</td>
                  <td className="px-4 py-3 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>{line.description || '-'}</td>
                  <td className="px-4 py-3 text-sm">
                    <button type="button" onClick={() => onRemove(index)} className="text-sm font-bold transition-colors duration-200 cursor-pointer hover:underline" style={{ color: 'var(--color-error)' }}>حذف</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editing && (
        <div className="mt-4 rounded-lg border p-5 space-y-4" style={{ backgroundColor: 'var(--color-surfaceContainerLow)', borderColor: 'var(--color-outlineVariant)' }}>
          <h4 className="text-sm font-bold" style={{ color: 'var(--color-onSurface)' }}>سطر جديد</h4>
          <div className="grid grid-cols-4 gap-4">
            <div className="col-span-2">
              <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>الحساب</label>
              <select value={editing.accountId || ''} onChange={(e) => setEditing({ ...editing, accountId: Number(e.target.value) })} className={inputCls} style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }}>
                <option value="">اختر الحساب...</option>
                {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} - {a.name}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>مدين</label>
              <input type="number" value={editing.debit || ''} onChange={(e) => setEditing({ ...editing, debit: Number(e.target.value), credit: 0 })} className={inputCls}
                style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
            </div>
            <div>
              <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>دائن</label>
              <input type="number" value={editing.credit || ''} onChange={(e) => setEditing({ ...editing, credit: Number(e.target.value), debit: 0 })} className={inputCls}
                style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
            </div>
          </div>
          <div>
            <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>الوصف</label>
            <input type="text" value={editing.description || ''} onChange={(e) => setEditing({ ...editing, description: e.target.value })} placeholder="وصف السطر..." className={inputCls}
              style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
          </div>
          <div>
            <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>مركز التكلفة</label>
            <select value={editing.costCenterId || ''} onChange={(e) => setEditing({ ...editing, costCenterId: e.target.value ? Number(e.target.value) : null })} className={inputCls}
              style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }}>
              <option value="">بدون</option>
              {costCenters.map((c) => <option key={c.id} value={c.id}>{c.code} - {c.name}</option>)}
            </select>
          </div>
          <div className="flex gap-2 pt-2">
            <button type="button" onClick={handleSave} className="px-4 py-2 text-sm font-bold rounded-lg transition-all duration-200 cursor-pointer hover:shadow-sm"
              style={{ backgroundColor: 'var(--color-success)', color: 'var(--color-surface)' }}>حفظ السطر</button>
            <button type="button" onClick={() => setEditing(null)} className="px-4 py-2 text-sm font-bold rounded-lg transition-all duration-200 cursor-pointer hover:shadow-sm"
              style={{ backgroundColor: 'var(--color-surfaceContainer)', color: 'var(--color-onSurface)' }}>إلغاء</button>
          </div>
        </div>
      )}

      <div className="mt-4"><BalanceIndicator totalDebit={totalDebit} totalCredit={totalCredit} /></div>
    </div>
  );
}
