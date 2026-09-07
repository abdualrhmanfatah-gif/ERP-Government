import { useState } from 'react';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCurrenciesList } from '@/features/accounting/hooks/useCurrenciesList';
import { useCreateTemplateLine, useUpdateTemplateLine, useRemoveTemplateLine } from '@/features/accounting/hooks/useTemplateLines';
import { showToast } from '@/components/ui/Toast';

export interface TemplateLineRow {
  id: number;
  sequence: number;
  accountCode?: string;
  accountName?: string;
  debit: number;
  credit: number;
  description?: string | null;
  currencyId: number;
  exchangeRate: number;
  costCenterId?: number | null;
  costCenterName?: string | null;
}

interface Props {
  templateId: number;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  lines: any[];
}

export function TemplateLinesSection({ templateId, lines }: Props) {
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const { data: currencies = [] } = useCurrenciesList();
  const createLine = useCreateTemplateLine();
  const updateLine = useUpdateTemplateLine();
  const removeLine = useRemoveTemplateLine();

  const [editing, setEditing] = useState<null | {
    id?: number;
    accountId: number;
    debit: number;
    credit: number;
    description: string;
    currencyId: number;
    exchangeRate: number;
    rowVersion?: string;
  }>(null);

  const rows: TemplateLineRow[] = (lines || []).map((l) => ({
    id: l.id,
    sequence: l.sequence,
    accountCode: l.accountCode,
    accountName: l.accountName,
    debit: l.debit,
    credit: l.credit,
    description: l.description,
    currencyId: l.currencyId,
    exchangeRate: l.exchangeRate,
    costCenterId: l.costCenterId,
    costCenterName: l.costCenterName,
  }));

  const totalDebit = rows.reduce((s, l) => s + (l.debit || 0), 0);
  const totalCredit = rows.reduce((s, l) => s + (l.credit || 0), 0);

  const openNew = () => setEditing({
    accountId: 0, debit: 0, credit: 0, description: '',
    currencyId: currencies[0]?.id ?? 1, exchangeRate: 1,
  });

  const openEdit = (row: TemplateLineRow) => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const full = (lines || []).find((l: any) => l.id === row.id) as any;
    setEditing({
      id: row.id, accountId: full?.accountId ?? 0,
      debit: row.debit, credit: row.credit, description: row.description || '',
      currencyId: row.currencyId, exchangeRate: row.exchangeRate,
      rowVersion: full?.rowVersion ?? '',
    });
  };

  const save = async () => {
    if (!editing) return;
    if (editing.debit > 0 && editing.credit > 0) { showToast('error', 'يجب أن يكون السطر مدين أو دائن فقط'); return; }
    if (!editing.debit && !editing.credit) { showToast('error', 'يجب إدخال مبلغ مدين أو دائن'); return; }
    if (!editing.accountId) { showToast('error', 'يجب اختيار الحساب'); return; }
    try {
      if (editing.id) {
        await updateLine.mutateAsync({
          templateId, lineId: editing.id,
          payload: {
            accountId: editing.accountId, description: editing.description,
            currencyId: editing.currencyId, exchangeRate: editing.exchangeRate,
            debit: editing.debit, credit: editing.credit,
            rowVersion: editing.rowVersion ?? '',
          },
        });
        showToast('success', 'تم تعديل السطر');
      } else {
        await createLine.mutateAsync({
          templateId,
          payload: {
            accountId: editing.accountId, description: editing.description,
            currencyId: editing.currencyId, exchangeRate: editing.exchangeRate,
            debit: editing.debit, credit: editing.credit,
          },
        });
        showToast('success', 'تمت إضافة السطر');
      }
      setEditing(null);
    } catch {
      showToast('error', 'فشلت العملية');
    }
  };

  const remove = async (row: TemplateLineRow) => {
    try {
      await removeLine.mutateAsync({ templateId, lineId: row.id });
      showToast('success', 'تم حذف السطر');
    } catch {
      showToast('error', 'فشل الحذف');
    }
  };

  const inputCls = 'w-full px-3 py-2 rounded-lg border text-sm focus:outline-none';
  const labelCls = 'block text-sm font-bold mb-1.5';

  return (
    <div>
      <div className="flex justify-end mb-4">
        <button type="button" onClick={openNew}
          className="px-4 py-2 text-sm font-bold rounded-lg cursor-pointer hover:shadow-sm"
          style={{ backgroundColor: 'var(--color-secondary-container)', color: 'var(--color-on-secondary-container)' }}>
          + إضافة سطر
        </button>
      </div>

      {rows.length === 0 ? (
        <div className="text-center py-12 rounded-lg border-2 border-dashed" style={{ borderColor: 'var(--color-outlineVariant)' }}>
          <p className="text-sm font-medium" style={{ color: 'var(--color-onSurfaceVariant)' }}>لا توجد أسطر بعد</p>
        </div>
      ) : (
        <div>
          <div className="overflow-x-auto rounded-lg border" style={{ borderColor: 'var(--color-outlineVariant)' }}>
            <table className="min-w-full divide-y" style={{ borderColor: 'var(--color-outlineVariant)' }}>
              <thead>
                <tr style={{ backgroundColor: 'var(--color-surfaceContainerLow)' }}>
                  <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>#</th>
                  <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>الحساب</th>
                  <th className="px-4 py-3 text-left text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>مدين</th>
                  <th className="px-4 py-3 text-left text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>دائن</th>
                  <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>الوصف</th>
                  <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>عملة/سعر</th>
                  <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>مركز تكلفة</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y" style={{ borderColor: 'var(--color-outlineVariant)' }}>
                {rows.map((line) => (
                  <tr key={line.id} style={{ backgroundColor: 'var(--color-surface)' }}>
                    <td className="px-4 py-3 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>{line.sequence}</td>
                    <td className="px-4 py-3 text-sm font-bold" style={{ color: 'var(--color-onSurface)' }}>
                      {line.accountCode} - {line.accountName}
                    </td>
                    <td className="px-4 py-3 text-sm text-left tabular-nums font-bold" style={{ color: 'var(--color-onSurface)' }}>
                      {line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}
                    </td>
                    <td className="px-4 py-3 text-sm text-left tabular-nums font-bold" style={{ color: 'var(--color-onSurface)' }}>
                      {line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>{line.description || '-'}</td>
                    <td className="px-4 py-3 text-xs" style={{ color: 'var(--color-onSurfaceVariant)' }}>
                      {line.currencyId} × {line.exchangeRate}
                    </td>
                    <td className="px-4 py-3 text-xs" style={{ color: 'var(--color-onSurfaceVariant)' }}>
                      {line.costCenterName || line.costCenterId || '-'}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex gap-2">
                        <button type="button" onClick={() => openEdit(line)} className="text-sm font-bold cursor-pointer hover:underline" style={{ color: 'var(--color-link)' }}>تعديل</button>
                        <button type="button" onClick={() => remove(line)} className="text-sm font-bold cursor-pointer hover:underline" style={{ color: 'var(--color-error)' }}>حذف</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-4"><BalanceIndicator totalDebit={totalDebit} totalCredit={totalCredit} /></div>
        </div>
      )}

      {editing && (
        <div className="mt-4 rounded-lg border p-5 space-y-4" style={{ backgroundColor: 'var(--color-surfaceContainerLow)', borderColor: 'var(--color-outlineVariant)' }}>
          <h4 className="text-sm font-bold" style={{ color: 'var(--color-onSurface)' }}>{editing.id ? 'تعديل سطر' : 'سطر جديد'}</h4>
          <div className="grid grid-cols-4 gap-4">
            <div className="col-span-2">
              <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>الحساب</label>
              <select value={editing.accountId || ''} onChange={(e) => setEditing({ ...editing, accountId: Number(e.target.value) })}
                className={inputCls} style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }}>
                <option value="">اختر الحساب...</option>
                {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} - {a.name}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>العملة</label>
              <select value={editing.currencyId || ''} onChange={(e) => setEditing({ ...editing, currencyId: Number(e.target.value) })}
                className={inputCls} style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }}>
                {currencies?.map((c) => <option key={c.id} value={c.id}>{c.code} - {c.name}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>سعر الصرف</label>
              <input type="number" step="any" value={editing.exchangeRate || ''} onChange={(e) => setEditing({ ...editing, exchangeRate: Number(e.target.value) })} className={inputCls}
                style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
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
            <div className="col-span-2">
              <label className={labelCls} style={{ color: 'var(--color-onSurface)' }}>الوصف</label>
              <input type="text" value={editing.description || ''} onChange={(e) => setEditing({ ...editing, description: e.target.value })} placeholder="وصف السطر..." className={inputCls}
                style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
            </div>
          </div>
          <div className="flex gap-2 pt-2">
            <button type="button" onClick={save} className="px-4 py-2 text-sm font-bold rounded-lg cursor-pointer hover:shadow-sm"
              style={{ backgroundColor: 'var(--color-success)', color: 'var(--color-surface)' }}>حفظ السطر</button>
            <button type="button" onClick={() => setEditing(null)} className="px-4 py-2 text-sm font-bold rounded-lg cursor-pointer hover:shadow-sm"
              style={{ backgroundColor: 'var(--color-surfaceContainer)', color: 'var(--color-onSurface)' }}>إلغاء</button>
          </div>
        </div>
      )}
    </div>
  );
}
