import { useState } from 'react';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCurrenciesList } from '@/features/accounting/hooks/useCurrenciesList';
import { useCreateTemplateLine, useUpdateTemplateLine, useRemoveTemplateLine } from '@/features/accounting/hooks/useTemplateLines';
import { showToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';

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

  return (
    <div>
      <div className="flex justify-end mb-4">
        <Button type="button" variant="secondary" size="sm" onClick={openNew}>
          + إضافة سطر
        </Button>
      </div>

      {rows.length === 0 ? (
        <div className="text-center py-12 rounded-lg border-2 border-dashed border-[var(--color-outline-variant)]">
          <p className="text-sm font-medium text-[var(--color-on-surface-variant)]">لا توجد أسطر بعد</p>
        </div>
      ) : (
        <div>
          <div className="overflow-x-auto rounded-lg border border-[var(--color-outline-variant)]">
            <table className="min-w-full divide-y border-[var(--color-outline-variant)]">
              <thead>
                <tr className="bg-[var(--color-surface-container-low)]">
                  <th className="px-4 py-3 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">#</th>
                  <th className="px-4 py-3 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">الحساب</th>
                  <th className="px-4 py-3 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">مدين</th>
                  <th className="px-4 py-3 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">دائن</th>
                  <th className="px-4 py-3 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">الوصف</th>
                  <th className="px-4 py-3 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">عملة/سعر</th>
                  <th className="px-4 py-3 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">مركز تكلفة</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y border-[var(--color-outline-variant)]">
                {rows.map((line) => (
                  <tr key={line.id} className="bg-[var(--color-surface)]">
                    <td className="px-4 py-3 text-sm text-[var(--color-on-surface-variant)]">{line.sequence}</td>
                    <td className="px-4 py-3 text-sm font-bold text-[var(--color-on-surface)]">
                      {line.accountCode} - {line.accountName}
                    </td>
                    <td className="px-4 py-3 text-sm text-start tabular-nums font-bold text-[var(--color-on-surface)]">
                      {line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}
                    </td>
                    <td className="px-4 py-3 text-sm text-start tabular-nums font-bold text-[var(--color-on-surface)]">
                      {line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}
                    </td>
                    <td className="px-4 py-3 text-sm text-[var(--color-on-surface-variant)]">{line.description || '-'}</td>
                    <td className="px-4 py-3 text-xs text-[var(--color-on-surface-variant)]">
                      {line.currencyId} × {line.exchangeRate}
                    </td>
                    <td className="px-4 py-3 text-xs text-[var(--color-on-surface-variant)]">
                      {line.costCenterName || line.costCenterId || '-'}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex gap-2">
                        <Button type="button" variant="link" size="sm" onClick={() => openEdit(line)}>تعديل</Button>
                        <Button type="button" variant="ghost" size="sm" onClick={() => remove(line)} className="text-[var(--color-error)] hover:text-[var(--color-error)]">حذف</Button>
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
        <div className="mt-4 rounded-lg border p-5 space-y-4 bg-[var(--color-surface-container-low)] border-[var(--color-outline-variant)]">
          <h4 className="text-sm font-bold text-[var(--color-on-surface)]">{editing.id ? 'تعديل سطر' : 'سطر جديد'}</h4>
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
              <Select
                label="العملة"
                value={editing.currencyId || ''}
                onChange={(e) => setEditing({ ...editing, currencyId: Number(e.target.value) })}
                options={currencies?.map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` })) ?? []}
              />
            </div>
            <div>
              <Input
                label="سعر الصرف"
                type="number"
                step="any"
                value={editing.exchangeRate || ''}
                onChange={(e) => setEditing({ ...editing, exchangeRate: Number(e.target.value) })}
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
            <div className="col-span-2">
              <Input
                label="الوصف"
                type="text"
                value={editing.description || ''}
                onChange={(e) => setEditing({ ...editing, description: e.target.value })}
                placeholder="وصف السطر..."
              />
            </div>
          </div>
          <div className="flex gap-2 pt-2">
            <Button type="button" variant="primary" size="sm" onClick={save}>حفظ السطر</Button>
            <Button type="button" variant="outline" size="sm" onClick={() => setEditing(null)}>إلغاء</Button>
          </div>
        </div>
      )}
    </div>
  );
}
