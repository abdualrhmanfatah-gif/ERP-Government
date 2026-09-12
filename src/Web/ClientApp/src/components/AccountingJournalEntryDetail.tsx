import { useEffect, useMemo, useState } from 'react';
import { useUpdateJournalEntry } from '@/features/accounting/hooks/useJournalEntries';
import {
  useCreateJournalEntryLine,
  useUpdateJournalEntryLine,
  useRemoveJournalEntryLine,
} from '@/features/accounting/hooks/useJournalEntryLines';
import { useJournalsList } from '@/features/accounting/hooks/useJournalsList';
import { useCurrenciesList } from '@/features/accounting/hooks/useCurrenciesList';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCostCenters } from '@/features/organization/hooks/useCostCenters';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';
import type { JournalEntryDto } from '../web-api-client';
import { notify } from '@/features/notifications/notify';
import { Button, Card, Combobox, Input, Select, Textarea, Badge } from '@/components/ui';
import { entryTypeLabels, manualEntryTypes } from '@/features/accounting/shared/types';
import { toDateInput } from '@/shared/utils/formatters';

interface EditLine {
  key: string;
  id: number;
  sequence: number;
  accountId: number;
  accountCode?: string;
  accountName?: string;
  description: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId: number | null;
  rowVersion?: string;
}

interface AccountingJournalEntryDetailProps {
  entry: JournalEntryDto;
  editing?: boolean;
  onToggleEditing?: (editing: boolean) => void;
  onStateChange?: (state: { canSave: boolean; isSaving: boolean }) => void;
  onSaved?: () => void;
}

export function AccountingJournalEntryDetail({ entry, editing = false, onToggleEditing, onStateChange, onSaved }: AccountingJournalEntryDetailProps) {
  const [ref, setRef] = useState(entry.ref ?? '');
  const [narration, setNarration] = useState(entry.narration ?? '');
  const [lines, setLines] = useState<EditLine[]>(() => toEditLines(entry.lines ?? []));
  const [editingLineIndex, setEditingLineIndex] = useState<number | null>(null);
  const [draftLine, setDraftLine] = useState<EditLine | null>(null);

  const updateEntry = useUpdateJournalEntry();
  const createLine = useCreateJournalEntryLine();
  const updateLine = useUpdateJournalEntryLine();
  const removeLine = useRemoveJournalEntryLine();

  const { data: journals } = useJournalsList({ isActive: true });
  const { data: currencies } = useCurrenciesList();
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const { data: costCenters = [] } = useCostCenters();

  const accountMap = useMemo(() => new Map(accounts.map((a) => [a.id, `${a.code} - ${a.name}`])), [accounts]);
  const costCenterMap = useMemo(() => new Map(costCenters.map((c) => [c.id, `${c.code} - ${c.name}`])), [costCenters]);

  const totalDebit = lines.reduce((s, l) => s + (l.debit || 0), 0);
  const totalCredit = lines.reduce((s, l) => s + (l.credit || 0), 0);
  const isBalanced = Math.abs(totalDebit - totalCredit) < 0.001;
  const canSave = lines.length > 0 && isBalanced;
  const isSaving = updateEntry.isPending || createLine.isPending || updateLine.isPending || removeLine.isPending;

  useEffect(() => {
    onStateChange?.({ canSave, isSaving });
  }, [canSave, isSaving, onStateChange]);

  const handleSave = async () => {
    if (!canSave) {
      notify({ type: 'error', title: 'يجب أن يتوازن القيد (مدين = دائن)' });
      return;
    }
    const initialLines = toEditLines(entry.lines ?? []);
    const currentIds = new Set(lines.map((l) => l.id));

    try {
      await updateEntry.mutateAsync({
        id: entry.id!,
        command: { narration, ref, rowVersion: entry.rowVersion ?? '' },
      });

      for (const line of lines) {
        if (line.id > 0) {
          await updateLine.mutateAsync({
            journalEntryId: entry.id!,
            lineId: line.id,
            command: {
              id: line.id,
              journalEntryId: entry.id!,
              accountId: line.accountId,
              description: line.description ?? null,
              currencyId: line.currencyId,
              exchangeRate: line.exchangeRate,
              debit: line.debit,
              credit: line.credit,
              costCenterId: line.costCenterId,
              rowVersion: line.rowVersion ?? '',
            },
          });
        } else {
          await createLine.mutateAsync({
            journalEntryId: entry.id!,
            command: {
              accountId: line.accountId,
              description: line.description ?? null,
              currencyId: line.currencyId,
              exchangeRate: line.exchangeRate,
              debit: line.debit,
              credit: line.credit,
              costCenterId: line.costCenterId,
            },
          });
        }
      }

      for (const line of initialLines) {
        if (line.id > 0 && !currentIds.has(line.id)) {
          await removeLine.mutateAsync({ journalEntryId: entry.id!, lineId: line.id });
        }
      }

      notify({ type: 'success', title: 'تم حفظ التعديلات' });
      onToggleEditing?.(false);
      onSaved?.();
    } catch (err: unknown) {
      const e = err as { status?: number };
      if (e?.status === 409) {
        notify({ type: 'error', title: 'تم تعديل القيد بواسطة مستخدم آخر. أعد تحميل البيانات' });
      } else {
        notify({ type: 'error', title: 'فشل حفظ التعديلات' });
      }
    }
  };

  const handleAddDraft = () => {
    const seq = lines.length > 0 ? Math.max(...lines.map((l) => l.sequence)) + 1 : 1;
    const draft: EditLine = {
      key: `new-${Date.now()}`,
      id: 0,
      sequence: seq,
      accountId: 0,
      description: null,
      currencyId: 1,
      exchangeRate: 1,
      debit: 0,
      credit: 0,
      costCenterId: null,
    };
    setEditingLineIndex(null);
    setDraftLine(draft);
  };

  const handleSaveDraft = () => {
    if (!draftLine) return;
    if (draftLine.debit > 0 && draftLine.credit > 0) { notify({ type: 'error', title: 'يجب أن يكون البيان مدين أو دائن فقط' }); return; }
    if (!draftLine.debit && !draftLine.credit) { notify({ type: 'error', title: 'يجب إدخال مبلغ مدين أو دائن' }); return; }
    if (!draftLine.accountId) { notify({ type: 'error', title: 'يجب اختيار الحساب' }); return; }

    if (editingLineIndex === null) {
      setLines([...lines, draftLine]);
    } else {
      setLines(lines.map((l, i) => (i === editingLineIndex ? draftLine : l)));
    }
    setDraftLine(null);
    setEditingLineIndex(null);
  };

  const handleEditLine = (index: number) => {
    setEditingLineIndex(index);
    setDraftLine(lines[index]);
  };

  const handleRemoveLine = (index: number) => {
    setLines(lines.filter((_, i) => i !== index));
    setEditingLineIndex(null);
    setDraftLine(null);
  };

  const viewTotalDebit = entry.totalBaseDebit ?? totalDebit;
  const viewTotalCredit = entry.totalBaseCredit ?? totalCredit;
  const dateInput = toDateInput(entry.documentDate);

  return (
    <form id="journal-entry-detail-form" onSubmit={(e) => { e.preventDefault(); void handleSave(); }} aria-label="تفاصيل قيد اليومية">
      <Card variant="default">
        <div className="px-4 py-2.5 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
          <h2 className="text-sm font-bold text-[var(--color-on-surface)]">بيانات القيد</h2>
        </div>

        <div className="p-4">
          <div className="grid grid-cols-5 gap-3">
            <Input
              id="detail-documentDate"
              type="date"
              label="التاريخ"
              value={dateInput}
              disabled
            />
            <Select
              id="detail-journalId"
              label="اليومية"
              value={entry.journalId ? String(entry.journalId) : ''}
              disabled
              options={[
                { value: '', label: entry.journalName || 'اختر يومية...' },
                ...(journals?.map((j) => ({ value: String(j.id), label: `${j.code} - ${j.name}` })) ?? []),
              ]}
            />
            <Select
              id="detail-entryType"
              label="نوع القيد"
              value={entry.entryType ? String(entry.entryType) : ''}
              disabled
              options={[
                { value: '', label: 'اختر النوع...' },
                ...manualEntryTypes.map((t) => ({ value: t, label: entryTypeLabels[t] })),
                ...(entry.entryType && !manualEntryTypes.includes(entry.entryType as any)
                  ? [{ value: String(entry.entryType), label: entryTypeLabels[entry.entryType as keyof typeof entryTypeLabels] || String(entry.entryType) }]
                  : []),
              ]}
            />
            <Select
              id="detail-baseCurrencyId"
              label="العملة"
              value={entry.baseCurrencyId ? String(entry.baseCurrencyId) : ''}
              disabled
              options={[
                { value: '', label: 'اختر العملة...' },
                ...(currencies?.map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` })) ?? []),
              ]}
            />
            <Input
              id="detail-ref"
              type="text"
              label="المرجع"
              placeholder="رقم المرجع..."
              value={ref}
              disabled={!editing}
              onChange={(e) => setRef(e.target.value)}
            />
          </div>
          <div className="mt-3">
            <Textarea
              id="detail-narration"
              label="البيان"
              rows={1}
              className="min-h-0"
              placeholder="بيان القيد..."
              value={narration}
              disabled={!editing}
              onChange={(e) => setNarration(e.target.value)}
            />
          </div>
        </div>

        <div className="px-4 py-2.5 border-t border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-bold text-[var(--color-on-surface)]">بيان القيد</h2>
            <div className="flex items-center gap-3">
              {lines.length > 0 && (
                <Badge variant={isBalanced ? 'success' : 'error'}>
                  {lines.length} بيان
                </Badge>
              )}
              {editing && (
                <Button type="button" variant="secondary" size="sm" onClick={handleAddDraft}>
                  + إضافة بيان
                </Button>
              )}
            </div>
          </div>
        </div>

        <div className="p-4">
          <div className="overflow-x-auto rounded-lg border border-[var(--color-outline-variant)]">
            <table className="min-w-full divide-y border-[var(--color-outline-variant)]">
              <thead>
                <tr className="bg-[var(--color-surface-container-low)]">
                  <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">#</th>
                  <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">الحساب</th>
                  <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">مركز التكلفة</th>
                  <th className="px-3 py-2 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">مدين</th>
                  <th className="px-3 py-2 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">دائن</th>
                  <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">البيان</th>
                  {editing && <th className="px-3 py-2" />}
                </tr>
              </thead>
              <tbody className="divide-y border-[var(--color-outline-variant)]">
                {lines.map((line, index) => (
                  <tr key={line.key} className="bg-[var(--color-surface)]">
                    <td className="px-3 py-2 text-xs text-[var(--color-on-surface-variant)]">{line.sequence}</td>
                    <td className="px-3 py-2 text-xs font-bold text-[var(--color-on-surface)]">
                      {accountMap.get(line.accountId) || (line.accountCode ? `${line.accountCode} - ${line.accountName}` : line.accountId)}
                    </td>
                    <td className="px-3 py-2 text-xs text-[var(--color-on-surface-variant)]">
                      {costCenterMap.get(line.costCenterId ?? 0) || '-'}
                    </td>
                    <td className="px-3 py-2 text-xs text-start tabular-nums font-bold text-[var(--color-on-surface)]">
                      {line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}
                    </td>
                    <td className="px-3 py-2 text-xs text-start tabular-nums font-bold text-[var(--color-on-surface)]">
                      {line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}
                    </td>
                    <td className="px-3 py-2 text-xs text-[var(--color-on-surface-variant)]">{line.description || '-'}</td>
                    {editing && (
                      <td className="px-3 py-2 text-xs">
                        <div className="flex justify-end gap-1">
                          <Button type="button" variant="ghost" size="sm" onClick={() => handleEditLine(index)}>تعديل</Button>
                          <Button type="button" variant="ghost" size="sm" onClick={() => handleRemoveLine(index)} className="text-[var(--color-error)] hover:text-[var(--color-error)]">حذف</Button>
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {editing && draftLine && (
            <div className="mt-3 rounded-lg border p-3 bg-[var(--color-surface-container-low)] border-[var(--color-outline-variant)]">
              <div className="grid grid-cols-6 gap-3">
                <div className="col-span-2">
                  <Combobox
                    label="الحساب"
                    value={draftLine.accountId ? String(draftLine.accountId) : ''}
                    onChange={(val) => setDraftLine({ ...draftLine, accountId: val ? Number(val) : 0 })}
                    options={accounts.map((a) => ({ value: String(a.id), label: `${a.code} - ${a.name}` }))}
                    placeholder="اختر الحساب..."
                    searchPlaceholder="بحث عن حساب بالرمز أو الاسم..."
                    emptyMessage="لا توجد نتائج"
                  />
                </div>
                <Input
                  label="مدين"
                  type="number"
                  value={draftLine.debit || ''}
                  onChange={(e) => setDraftLine({ ...draftLine, debit: Number(e.target.value), credit: 0 })}
                />
                <Input
                  label="دائن"
                  type="number"
                  value={draftLine.credit || ''}
                  onChange={(e) => setDraftLine({ ...draftLine, credit: Number(e.target.value), debit: 0 })}
                />
                <Select
                  label="مركز التكلفة"
                  value={draftLine.costCenterId ? String(draftLine.costCenterId) : ''}
                  onChange={(e) => setDraftLine({ ...draftLine, costCenterId: e.target.value ? Number(e.target.value) : null })}
                  options={[
                    { value: '', label: 'بدون' },
                    ...costCenters.map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` })),
                  ]}
                />
                <Input
                  label="البيان"
                  type="text"
                  value={draftLine.description ?? ''}
                  onChange={(e) => setDraftLine({ ...draftLine, description: e.target.value })}
                  placeholder="بيان السطر..."
                />
              </div>
              <div className="flex justify-end gap-2 mt-3">
                <Button type="button" variant="outline" size="sm" onClick={() => { setDraftLine(null); setEditingLineIndex(null); }}>إلغاء</Button>
                <Button type="button" variant="primary" size="sm" onClick={handleSaveDraft}>حفظ</Button>
              </div>
            </div>
          )}

          <div className="mt-3">
            <BalanceIndicator
              totalDebit={editing ? totalDebit : viewTotalDebit}
              totalCredit={editing ? totalCredit : viewTotalCredit}
            />
          </div>
        </div>
      </Card>
    </form>
  );
}

function toEditLines(lines: JournalEntryDto['lines']): EditLine[] {
  return (lines ?? []).map((l) => ({
    key: `existing-${l.id}`,
    id: l.id ?? 0,
    sequence: l.sequence ?? 0,
    accountId: l.accountId ?? 0,
    accountCode: l.accountCode,
    accountName: l.accountName,
    description: l.description ?? null,
    currencyId: l.currencyId ?? 1,
    exchangeRate: l.exchangeRate ?? 1,
    debit: l.debit ?? 0,
    credit: l.credit ?? 0,
    costCenterId: l.costCenterId ?? null,
    rowVersion: l.rowVersion,
  }));
}