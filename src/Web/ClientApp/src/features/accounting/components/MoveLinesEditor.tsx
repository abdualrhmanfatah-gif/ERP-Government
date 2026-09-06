import { useState, useCallback, Fragment } from 'react';
import { Button } from '@/components/ui/Button';
import type { MoveLineDto } from '../types';
import { useCreateMoveLine, useUpdateMoveLine, useRemoveMoveLine } from '../hooks/useMoveLines';
import { useAccountsList } from '../hooks/useAccountsList';
import { useCurrenciesList } from '../hooks/useCurrenciesList';
import { useCostCenters } from '../../organization/hooks/useCostCenters';
import { BalanceIndicator } from './BalanceIndicator';

interface MoveLinesEditorProps {
  moveId: number;
  lines: MoveLineDto[];
  entryStatus: string;
  documentDate: string;
}

export function MoveLinesEditor({ moveId, lines, entryStatus, documentDate }: MoveLinesEditorProps) {
  const isDraft = entryStatus === 'Draft';
  const createLine = useCreateMoveLine();
  const removeLine = useRemoveMoveLine();

  const { data: accountsData } = useAccountsList({ isPostable: true, isActive: true });
  const { data: currenciesData } = useCurrenciesList({ isActive: true });
  const { data: costCentersData } = useCostCenters();

  const accounts = accountsData ?? [];
  const currencies = currenciesData ?? [];
  const costCenters = costCentersData ?? [];
  const baseCurrencyId = currencies.find((c) => c.isBase)?.id ?? 1;

  const [editingLineId, setEditingLineId] = useState<number | null>(null);
  const [editForm, setEditForm] = useState<{
    accountId: string;
    currencyId: string;
    costCenterId: string;
    exchangeRate: string;
    debit: string;
    credit: string;
  }>({ accountId: '', currencyId: '', costCenterId: '', exchangeRate: '', debit: '', credit: '' });

  const updateLine = useUpdateMoveLine();

  const [form, setForm] = useState({
    accountId: '',
    description: '',
    currencyId: '',
    costCenterId: '',
    exchangeRate: '1',
    debit: '',
    credit: '',
  });

  // ─── جلب سعر الصرف تلقائياً ─────────────────────────────
  const fetchExchangeRate = useCallback(async (currencyId: string, target: 'add' | 'edit') => {
    if (!currencyId || Number(currencyId) === baseCurrencyId) {
      if (target === 'add') setForm((prev) => ({ ...prev, exchangeRate: '1' }));
      else setEditForm((prev) => ({ ...prev, exchangeRate: '1' }));
      return;
    }
    try {
      const res = await fetch(
        `/api/ExchangeRates/lookup?BaseCurrencyId=${baseCurrencyId}&CurrencyId=${currencyId}&Date=${documentDate}&RateType=Official`,
        { headers: { Accept: 'application/json' } }
      );
      if (res.ok) {
        const data = await res.json();
        if (data?.rate) {
          if (target === 'add') setForm((prev) => ({ ...prev, exchangeRate: data.rate.toString() }));
          else setEditForm((prev) => ({ ...prev, exchangeRate: data.rate.toString() }));
        }
      }
    } catch {
      // ignore
    }
  }, [documentDate, baseCurrencyId]);

  const handleAdd = async () => {
    const debit = parseFloat(form.debit) || 0;
    const credit = parseFloat(form.credit) || 0;
    if (!form.accountId || (debit === 0 && credit === 0)) return;
    await createLine.mutateAsync({
      moveId,
      data: {
        accountId: Number(form.accountId),
        description: form.description || null,
        currencyId: Number(form.currencyId),
        exchangeRate: Number(form.exchangeRate),
        debit,
        credit,
      },
    });
    setForm({ accountId: '', description: '', currencyId: '', costCenterId: '', exchangeRate: '1', debit: '', credit: '' });
  };

  const handleRemove = async (lineId: number) => {
    await removeLine.mutateAsync({ moveId, lineId });
  };

  const handleRowClick = (line: MoveLineDto) => {
    if (!isDraft || editingLineId === line.id) return;
    setEditingLineId(line.id);
    setEditForm({
      accountId: line.accountId.toString(),
      currencyId: line.currencyId.toString(),
      costCenterId: line.costCenterId?.toString() ?? '',
      exchangeRate: line.exchangeRate.toString(),
      debit: line.debit.toString(),
      credit: line.credit.toString(),
    });
  };

  const handleSave = async () => {
    if (editingLineId === null) return;
    const line = lines.find((l) => l.id === editingLineId);
    if (!line) return;
    await updateLine.mutateAsync({
      moveId,
      lineId: editingLineId,
      data: {
        id: editingLineId,
        moveId,
        accountId: Number(editForm.accountId) || line.accountId,
        description: line.description ?? null,
        currencyId: Number(editForm.currencyId) || line.currencyId,
        exchangeRate: Number(editForm.exchangeRate) || line.exchangeRate,
        debit: Number(editForm.debit) || line.debit,
        credit: Number(editForm.credit) || line.credit,
        rowVersion: line.rowVersion,
      },
    });
    setEditingLineId(null);
  };

  const handleCancel = () => {
    setEditingLineId(null);
  };

  const getCurrencyLabel = (id: number) => {
    const cur = currencies.find((c) => c.id === id);
    return cur ? `${cur.code}` : id.toString();
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-[var(--color-primary)]">السطور</h3>
      </div>

      <div className="overflow-x-auto border border-[var(--color-primary-container)] rounded-xl">
        <table role="table" aria-label="سطور القيد" className="w-full border-collapse text-sm leading-relaxed">
          <thead>
            <tr>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-start">م</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-start">الحساب</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-start">البيان</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-end tabular-nums">مدين</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-end tabular-nums">دائن</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-end tabular-nums">سعر الصرف</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-center">العملة</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-center">مركز التكلفة</th>
              <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-b border-[var(--color-primary-container)] whitespace-nowrap text-center">إجراء</th>
            </tr>
          </thead>
          <tbody>
            {lines.map((l, idx) => {
              const isEditing = editingLineId === l.id;
              return (
                <Fragment key={l.id}>
                  <tr
                    onClick={() => isDraft && handleRowClick(l)}
                    className={`${idx % 2 === 1 ? 'bg-[rgba(0,32,69,0.06)]' : 'bg-[var(--color-surface-container-lowest)]'} ${isDraft ? 'cursor-pointer' : ''} transition-all duration-150 hover:bg-[color-mix(in_srgb,var(--color-primary-container)_5%,transparent)]`}
                  >
                    <td className="px-3 py-1.5 tabular-nums">{l.sequence}</td>
                    <td className="px-3 py-1.5">
                      <span className="font-medium">{l.accountCode}</span> — {l.accountName}
                    </td>
                    <td className="px-3 py-1.5">{l.description ?? '—'}</td>
                    <td className="px-3 py-1.5 text-end tabular-nums">{l.debit.toFixed(2)}</td>
                    <td className="px-3 py-1.5 text-end tabular-nums">{l.credit.toFixed(2)}</td>
                    <td className="px-3 py-1.5 text-end tabular-nums">{l.currencyId === baseCurrencyId ? '—' : l.exchangeRate.toFixed(6)}</td>
                    <td className="px-3 py-1.5 text-center">{getCurrencyLabel(l.currencyId)}</td>
                    <td className="px-3 py-1.5 text-center">{l.costCenterName ?? '—'}</td>
                    <td className="px-3 py-1.5 text-center">
                      {isDraft ? (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={(e) => { e.stopPropagation(); handleRemove(l.id); }}
                          aria-label={`حذف سطر ${l.sequence}`}
                          className="text-[var(--color-error)]"
                        >
                          حذف
                        </Button>
                      ) : (
                        <span className="text-[var(--color-outline)] text-xs">—</span>
                      )}
                    </td>
                  </tr>
                  {isEditing ? (
                    <tr className="bg-[var(--color-surface-container-low)]">
                      <td colSpan={9} className="px-3 py-3">
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-xs">
                           <div>
                              <label htmlFor="edit-account" className="block font-semibold text-on-surface-variant mb-1">الحساب</label>
                             <select
                               id="edit-account"
                               value={editForm.accountId}
                               onChange={(e) => setEditForm({ ...editForm, accountId: e.target.value })}
                                className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
                              >
                               <option value="">اختر الحساب</option>
                              {accounts.map((a) => (
                                <option key={a.id} value={a.id}>{a.code} — {a.name}</option>
                              ))}
                            </select>
                          </div>
                           <div>
                              <label htmlFor="edit-currency" className="block font-semibold text-on-surface-variant mb-1">العملة</label>
                             <select
                               id="edit-currency"
                               value={editForm.currencyId}
                              onChange={(e) => {
                                setEditForm({ ...editForm, currencyId: e.target.value });
                                fetchExchangeRate(e.target.value, 'edit');
                              }}
                               className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
                            >
                              <option value="">اختر العملة</option>
                              {currencies.map((c) => (
                                <option key={c.id} value={c.id}>{c.code} — {c.name}</option>
                              ))}
                            </select>
                          </div>
                           <div>
                              <label htmlFor="edit-debit" className="block font-semibold text-on-surface-variant mb-1">مدين</label>
                             <input
                               id="edit-debit"
                               type="number"
                              step="0.01"
                               value={editForm.debit}
                               onChange={(e) => setEditForm({ ...editForm, debit: e.target.value })}
                               className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm tabular-nums bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
                            />
                          </div>
                           <div>
                              <label htmlFor="edit-credit" className="block font-semibold text-on-surface-variant mb-1">دائن</label>
                             <input
                               id="edit-credit"
                               type="number"
                              step="0.01"
                               value={editForm.credit}
                               onChange={(e) => setEditForm({ ...editForm, credit: e.target.value })}
                               className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm tabular-nums bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
                            />
                          </div>
                           <div>
                               <label htmlFor="edit-cost-center" className="block font-semibold text-on-surface-variant mb-1">مركز التكلفة</label>
                              <select
                                id="edit-cost-center"
                                value={editForm.costCenterId}
                               onChange={(e) => setEditForm({ ...editForm, costCenterId: e.target.value })}
                              className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
                             >
                               <option value="">بدون</option>
                               {costCenters.map((cc) => (
                                 <option key={cc.id} value={cc.id}>{cc.code} — {cc.name}</option>
                               ))}
                             </select>
                           </div>
                         </div>
                        <div className="flex gap-2 mt-3">
                          <Button variant="primary" size="sm" onClick={(e) => { e.stopPropagation(); handleSave(); }} disabled={updateLine.isPending} loading={updateLine.isPending}>
                            حفظ
                          </Button>
                          <Button variant="outline" size="sm" onClick={(e) => { e.stopPropagation(); handleCancel(); }}>
                            إلغاء
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ) : null}
                </Fragment>
              );
            })}
            {lines.length === 0 ? (
              <tr>
                <td colSpan={9} className="px-3 py-8 text-center text-on-surface-variant text-xs">
                  لا توجد سطور بعد
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>

      {isDraft ? (
        <div className="border border-[var(--color-primary-container)] rounded-xl p-4 bg-[var(--color-surface-container-lowest)] space-y-3">
          <h4 className="text-sm font-semibold text-[var(--color-primary)]">إضافة سطر جديد</h4>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
            <div>
              <label className="block text-xs font-semibold text-on-surface-variant mb-1">الحساب *</label>
              <select
                value={form.accountId}
                onChange={(e) => setForm({ ...form, accountId: e.target.value })}
                aria-label="الحساب"
                className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
              >
                <option value="">اختر الحساب</option>
                {accounts.map((a) => (
                  <option key={a.id} value={a.id}>{a.code} — {a.name}</option>
                ))}
              </select>
            </div>
            <div>
              <label htmlFor="add-debit" className="block text-xs font-semibold text-on-surface-variant mb-1">مدين</label>
              <input
                id="add-debit"
                type="number"
                step="0.01"
                value={form.debit}
                onChange={(e) => setForm({ ...form, debit: e.target.value })}
                className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm tabular-nums bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
              />
            </div>
            <div>
              <label htmlFor="add-credit" className="block text-xs font-semibold text-on-surface-variant mb-1">دائن</label>
              <input
                id="add-credit"
                type="number"
                step="0.01"
                value={form.credit}
                onChange={(e) => setForm({ ...form, credit: e.target.value })}
                className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm tabular-nums bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
              />
            </div>
            <div>
              <label htmlFor="add-currency" className="block text-xs font-semibold text-on-surface-variant mb-1">العملة</label>
              <select
                id="add-currency"
                value={form.currencyId}
                onChange={(e) => {
                  setForm({ ...form, currencyId: e.target.value });
                  fetchExchangeRate(e.target.value, 'add');
                }}
                className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
              >
                <option value="">اختر العملة</option>
                {currencies.map((c) => (
                  <option key={c.id} value={c.id}>{c.code} — {c.name}</option>
                ))}
              </select>
            </div>
            <div className="md:col-span-3">
              <label htmlFor="add-description" className="block text-xs font-semibold text-on-surface-variant mb-1">البيان</label>
              <input
                id="add-description"
                type="text"
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-sm bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
              />
            </div>
            <div className="flex items-center gap-2">
              <label htmlFor="add-cost-center" className="text-xs font-semibold text-on-surface-variant">مركز التكلفة</label>
              <select
                id="add-cost-center"
                value={form.costCenterId}
                onChange={(e) => setForm({ ...form, costCenterId: e.target.value })}
                className="w-full px-2 py-1.5 border border-[var(--color-border-container)] rounded-lg text-xs bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
              >
                <option value="">بدون</option>
                {costCenters.map((cc) => (
                  <option key={cc.id} value={cc.id}>{cc.code} — {cc.name}</option>
                ))}
              </select>
            </div>
          </div>
          <div className="flex justify-end">
            <Button variant="primary" size="sm" onClick={handleAdd} disabled={createLine.isPending} loading={createLine.isPending}>
              {createLine.isPending ? 'جاري الإضافة...' : 'إضافة سطر'}
            </Button>
          </div>
          {createLine.isError ? (
            <p role="alert" className="text-xs text-[var(--color-error)]">
              {(createLine.error as Error)?.message ?? 'خطأ في إضافة السطر'}
            </p>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}
