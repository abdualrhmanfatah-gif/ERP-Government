import { useEffect, useMemo, useRef, useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { useCreateJournalEntry, useUpdateJournalEntry } from '@/features/accounting/hooks/useJournalEntries';
import {
  useCreateJournalEntryLine,
  useRemoveJournalEntryLine,
  useUpdateJournalEntryLine,
} from '@/features/accounting/hooks/useJournalEntryLines';
import { useFiscalYearByDate } from '@/features/accounting/hooks/useFiscalYearByDate';
import { useJournalsList } from '@/features/accounting/hooks/useJournalsList';
import { useCurrenciesList } from '@/features/accounting/hooks/useCurrenciesList';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCostCenters } from '@/features/organization/hooks/useCostCenters';
import { FiscalYearIndicator } from '@/components/AccountingFiscalYearIndicator';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';
import { notify } from '@/features/notifications/notify';
import { Button, Card, Combobox, DataGrid, Input, MoneyDisplay, Select, Textarea, Badge } from '@/components/ui';
import { entryTypeLabels, manualEntryTypes } from '@/features/accounting/shared/types';
import { MoveEntryType, type JournalEntryDto } from '@/web-api-client';
import { toDateInput } from '@/shared/utils/formatters';

export type AccountingJournalEntryFormMode = 'create' | 'edit' | 'detail';

const headerSchema = z.object({
  documentDate: z.string().min(1, 'التاريخ مطلوب'),
  journalId: z.number().min(1, 'اختر يومية'),
  baseCurrencyId: z.number().min(1, 'اختر العملة'),
  narration: z.string().optional().nullable(),
  ref: z.string().optional().nullable(),
  entryType: z.string().min(1, 'اختر النوع'),
});

type HeaderFormData = z.infer<typeof headerSchema>;

interface EntryLine {
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

interface AccountingJournalEntryFormProps {
  mode: AccountingJournalEntryFormMode;
  entry?: JournalEntryDto;
  onSuccess?: (entryId: number) => void;
  onSaved?: () => void;
  onStateChange?: (state: { canSave: boolean; isSaving: boolean }) => void;
}

function toEntryLines(lines?: JournalEntryDto['lines']): EntryLine[] {
  return (lines ?? []).map((line) => ({
    key: `existing-${line.id}`,
    id: line.id ?? 0,
    sequence: line.sequence ?? 0,
    accountId: line.accountId ?? 0,
    accountCode: line.accountCode,
    accountName: line.accountName,
    description: line.description ?? null,
    currencyId: line.currencyId ?? 1,
    exchangeRate: line.exchangeRate ?? 1,
    debit: line.debit ?? 0,
    credit: line.credit ?? 0,
    costCenterId: line.costCenterId ?? null,
    rowVersion: line.rowVersion,
  }));
}

function toHeaderValues(entry?: JournalEntryDto): HeaderFormData {
  return {
    documentDate: entry ? toDateInput(entry.documentDate) : new Date().toISOString().split('T')[0],
    journalId: entry?.journalId ?? 0,
    baseCurrencyId: entry?.baseCurrencyId ?? 0,
    entryType: entry?.entryType != null ? String(entry.entryType) : '',
    narration: entry?.narration ?? null,
    ref: entry?.ref ?? null,
  };
}

export function AccountingJournalEntryForm({ mode, entry, onSuccess, onSaved, onStateChange }: AccountingJournalEntryFormProps) {
  const isCreate = mode === 'create';
  const isEditable = mode !== 'detail';

  const createEntry = useCreateJournalEntry();
  const updateEntry = useUpdateJournalEntry();
  const createLine = useCreateJournalEntryLine();
  const updateLine = useUpdateJournalEntryLine();
  const removeLine = useRemoveJournalEntryLine();

  const [lines, setLines] = useState<EntryLine[]>(() => toEntryLines(entry?.lines));
  const [editingLine, setEditingLine] = useState<EntryLine | null>(null);
  const tempLineKey = useRef(0);

  const { data: journals } = useJournalsList({ isActive: true });
  const { data: currencies } = useCurrenciesList();
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const { data: costCenters = [] } = useCostCenters();

  const accountMap = useMemo(() => new Map(accounts.map((a) => [a.id, `${a.code} - ${a.name}`])), [accounts]);
  const costCenterMap = useMemo(() => new Map(costCenters.map((c) => [c.id, `${c.code} - ${c.name}`])), [costCenters]);

  const {
    control,
    handleSubmit,
    getValues,
    setValue,
    watch,
    formState: { errors },
  } = useForm<HeaderFormData>({
    resolver: zodResolver(headerSchema),
    defaultValues: toHeaderValues(entry),
  });

  const watchedDate = watch('documentDate');
  const fiscalData = useFiscalYearByDate(watchedDate);

  const baseCurrency = currencies?.find((c) => c.isBase);

  useEffect(() => {
    if (isCreate && baseCurrency?.id && !getValues('baseCurrencyId')) {
      setValue('baseCurrencyId', baseCurrency.id, { shouldValidate: true });
    }
  }, [isCreate, baseCurrency, getValues, setValue]);

  const computedDebit = lines.reduce((sum, line) => sum + (line.debit || 0), 0);
  const computedCredit = lines.reduce((sum, line) => sum + (line.credit || 0), 0);
  const isBalanced = Math.abs(computedDebit - computedCredit) < 0.001;
  const canSave = isEditable && lines.length > 0 && isBalanced;
  const isSaving = createEntry.isPending || updateEntry.isPending || createLine.isPending || updateLine.isPending || removeLine.isPending;

  useEffect(() => {
    onStateChange?.({ canSave, isSaving });
  }, [canSave, isSaving, onStateChange]);

  const totalDebit = !isEditable ? entry?.totalBaseDebit ?? computedDebit : computedDebit;
  const totalCredit = !isEditable ? entry?.totalBaseCredit ?? computedCredit : computedCredit;

  const handleAddLine = () => {
    tempLineKey.current += 1;
    setEditingLine({
      key: `new-${tempLineKey.current}`,
      id: 0,
      sequence: lines.length > 0 ? Math.max(...lines.map((line) => line.sequence)) + 1 : 1,
      accountId: 0,
      description: null,
      currencyId: entry?.baseCurrencyId ?? 1,
      exchangeRate: 1,
      debit: 0,
      credit: 0,
      costCenterId: null,
    });
  };

  const handleEditLine = (line: EntryLine) => setEditingLine(line);

  const handleSaveLine = () => {
    if (!editingLine) return;
    if (editingLine.debit > 0 && editingLine.credit > 0) { notify({ type: 'error', title: 'يجب أن يكون البيان مدين أو دائن فقط' }); return; }
    if (!editingLine.debit && !editingLine.credit) { notify({ type: 'error', title: 'يجب إدخال مبلغ مدين أو دائن' }); return; }
    if (!editingLine.accountId) { notify({ type: 'error', title: 'يجب اختيار الحساب' }); return; }

    setLines((current) =>
      current.some((line) => line.key === editingLine.key)
        ? current.map((line) => (line.key === editingLine.key ? editingLine : line))
        : [...current, editingLine]
    );
    setEditingLine(null);
  };

  const handleRemoveLine = (key: string) => {
    setLines((current) => current.filter((line) => line.key !== key));
    setEditingLine((current) => (current?.key === key ? null : current));
  };

  const handleError = (err: unknown, fallback: string) => {
    const status = (err as { status?: number })?.status;
    notify({ type: 'error', title: status === 409 ? 'تم تعديل القيد بواسطة مستخدم آخر. أعد تحميل البيانات' : fallback });
  };

  const onSubmit = async (data: HeaderFormData) => {
    if (!isEditable) return;

    try {
      if (isCreate) {
        if (!fiscalData.data) { notify({ type: 'error', title: 'لا توجد سنة مالية مفتوحة لهذا التاريخ' }); return; }
        const newEntryId = await createEntry.mutateAsync({
          documentDate: data.documentDate,
          journalId: data.journalId,
          periodId: fiscalData.data.fiscalPeriodId,
          fiscalYearId: fiscalData.data.fiscalYearId,
          baseCurrencyId: data.baseCurrencyId,
          narration: data.narration,
          ref: data.ref,
          entryType: data.entryType as MoveEntryType,
        });
        for (const line of lines) {
          await createLine.mutateAsync({
            journalEntryId: newEntryId,
            command: {
              accountId: line.accountId,
              description: line.description,
              currencyId: line.currencyId,
              exchangeRate: line.exchangeRate,
              debit: line.debit,
              credit: line.credit,
              costCenterId: line.costCenterId,
            },
          });
        }
        onSuccess?.(newEntryId);
        return;
      }

      if (!entry?.id) return;
      const initialLines = toEntryLines(entry.lines);
      const currentIds = new Set(lines.filter((line) => line.id > 0).map((line) => line.id));

      await updateEntry.mutateAsync({
        id: entry.id,
        command: { id: entry.id, narration: data.narration, ref: data.ref, rowVersion: entry.rowVersion ?? '' },
      });

      for (const line of lines) {
        if (line.id > 0) {
          await updateLine.mutateAsync({
            journalEntryId: entry.id,
            lineId: line.id,
            command: {
              id: line.id,
              journalEntryId: entry.id,
              accountId: line.accountId,
              description: line.description,
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
            journalEntryId: entry.id,
            command: {
              accountId: line.accountId,
              description: line.description,
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
          await removeLine.mutateAsync({ journalEntryId: entry.id, lineId: line.id });
        }
      }

      notify({ type: 'success', title: 'تم حفظ التعديلات' });
      onSaved?.();
    } catch (err) {
      handleError(err, isCreate ? 'فشل حفظ القيد' : 'فشل حفظ التعديلات');
    }
  };

  const entryTypeOptions = useMemo(() => {
    const options = [
      { value: '', label: 'اختر النوع...' },
      ...manualEntryTypes.map((type) => ({ value: type, label: entryTypeLabels[type] })),
    ];
    if (entry?.entryType != null && !manualEntryTypes.some((type) => String(type) === String(entry.entryType))) {
      const rawType = entry.entryType as unknown as MoveEntryType;
      options.push({ value: String(entry.entryType), label: entryTypeLabels[rawType] ?? String(entry.entryType) });
    }
    return options;
  }, [entry?.entryType]);

  const journalOptions = useMemo(() => {
    const options = [
      { value: '', label: 'اختر يومية...' },
      ...(journals?.map((journal) => ({ value: String(journal.id), label: `${journal.code} - ${journal.name}` })) ?? []),
    ];
    if (entry?.journalId && !options.some((option) => option.value === String(entry.journalId))) {
      options.push({ value: String(entry.journalId), label: entry.journalName || String(entry.journalId) });
    }
    return options;
  }, [journals, entry?.journalId, entry?.journalName]);

  const currencyOptions = useMemo(() => [
    { value: '', label: 'اختر العملة...' },
    ...(currencies?.map((currency) => ({ value: String(currency.id), label: `${currency.code} - ${currency.name}` })) ?? []),
  ], [currencies]);

  return (
    <form id="journal-entry-form" onSubmit={handleSubmit(onSubmit)} aria-label="بيانات قيد اليومية">
      <Card variant="default">
        <div className="p-4">
          <div className="grid grid-cols-5 gap-3">
            <Controller
              name="documentDate"
              control={control}
              render={({ field }) => (
                <Input
                  id="documentDate"
                  type="date"
                  label="التاريخ"
                  value={field.value}
                  onChange={field.onChange}
                  disabled={!isCreate}
                  error={errors.documentDate?.message}
                  required
                />
              )}
            />
            <Controller
              name="journalId"
              control={control}
              render={({ field }) => (
                <Select
                  id="journalId"
                  label="اليومية"
                  value={field.value ? String(field.value) : ''}
                  onChange={(event) => field.onChange(event.target.value ? Number(event.target.value) : 0)}
                  disabled={!isCreate}
                  options={journalOptions}
                  error={errors.journalId?.message}
                />
              )}
            />
            <Controller
              name="entryType"
              control={control}
              render={({ field }) => (
                <Select
                  id="entryType"
                  label="نوع القيد"
                  value={field.value}
                  onChange={(event) => field.onChange(event.target.value)}
                  disabled={!isCreate}
                  options={entryTypeOptions}
                  error={errors.entryType?.message}
                />
              )}
            />
            <Controller
              name="baseCurrencyId"
              control={control}
              render={({ field }) => (
                <Select
                  id="baseCurrencyId"
                  label="العملة"
                  value={field.value ? String(field.value) : ''}
                  onChange={(event) => field.onChange(event.target.value ? Number(event.target.value) : 0)}
                  disabled={!isCreate}
                  options={currencyOptions}
                  error={errors.baseCurrencyId?.message}
                />
              )}
            />
            <Controller
              name="ref"
              control={control}
              render={({ field }) => (
                <Input
                  id="ref"
                  type="text"
                  label="المرجع"
                  placeholder="رقم المرجع..."
                  value={field.value ?? ''}
                  onChange={field.onChange}
                  disabled={!isEditable}
                />
              )}
            />
          </div>
          <div className="mt-3">
            <Controller
              name="narration"
              control={control}
              render={({ field }) => (
                <Textarea
                  id="narration"
                  label="البيان"
                  rows={1}
                  className="min-h-0"
                  placeholder="بنود القيد..."
                  value={field.value ?? ''}
                  onChange={field.onChange}
                  disabled={!isEditable}
                />
              )}
            />
          </div>
          {isCreate && watchedDate && (
            <div className="mt-3">
              <FiscalYearIndicator date={watchedDate} />
            </div>
          )}
        </div>

        <div className="px-4 py-2.5 border-t border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
          <div className="flex items-center justify-between">
            <h2 className="text-label-md text-[var(--color-on-surface)]">بنود القيد</h2>
            <div className="flex items-center gap-3">
              {lines.length > 0 && (
                <Badge variant="outline">{lines.length} بيان</Badge>
              )}
              {isEditable && (
                <Button type="button" variant="outline" size="sm" icon={<Plus size={16} />} onClick={handleAddLine}>
                  إضافة بيان
                </Button>
              )}
            </div>
          </div>
        </div>

        <div className="p-4">
          <DataGrid<EntryLine>
            data={lines}
            rowKey={(line) => line.key}
            emptyMessage="لا يوجد بيان بعد"
            columns={[
              { id: 'sequence', accessorKey: 'sequence', header: '#', width: 56, align: 'center' },
              {
                id: 'accountId',
                accessorKey: 'accountId',
                header: 'الحساب',
                cell: (line) => accountMap.get(line.accountId) ?? (line.accountCode ? `${line.accountCode} - ${line.accountName ?? ''}` : String(line.accountId)),
              },
              { id: 'costCenterId', accessorKey: 'costCenterId', header: 'مركز التكلفة', cell: (line) => (line.costCenterId ? costCenterMap.get(line.costCenterId) ?? '—' : '—') },
              { id: 'debit', accessorKey: 'debit', header: 'مدين', align: 'right', cell: (line) => line.debit > 0 ? <MoneyDisplay value={line.debit} /> : '—' },
              { id: 'credit', accessorKey: 'credit', header: 'دائن', align: 'right', cell: (line) => line.credit > 0 ? <MoneyDisplay value={line.credit} /> : '—' },
              { id: 'description', accessorKey: 'description', header: 'البيان', cell: (line) => line.description || '—' },
              ...(isEditable ? [{
                id: 'actions',
                header: 'إجراء',
                width: 110,
                align: 'center' as const,
                cell: (line: EntryLine) => (
                  <div className="flex justify-center gap-1">
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      icon={<Pencil size={14} />}
                      aria-label={`تعديل البيان ${line.sequence}`}
                      onClick={() => handleEditLine(line)}
                    />
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      icon={<Trash2 size={14} />}
                      aria-label={`حذف البيان ${line.sequence}`}
                      className="text-[var(--color-error)] hover:text-[var(--color-error)]"
                      onClick={() => handleRemoveLine(line.key)}
                    />
                  </div>
                ),
              }] : []),
            ]}
          />

          {isEditable && editingLine && (
            <div className="mt-3 rounded-lg border p-3 bg-[var(--color-surface-container-low)] border-[var(--color-outline-variant)]">
              <div className="grid grid-cols-6 gap-3">
                <div className="col-span-2">
                  <Combobox
                    label="الحساب"
                    value={editingLine.accountId ? String(editingLine.accountId) : ''}
                    onChange={(val) => setEditingLine({ ...editingLine, accountId: val ? Number(val) : 0 })}
                    options={accounts.map((account) => ({ value: String(account.id), label: `${account.code} - ${account.name}` }))}
                    placeholder="اختر الحساب..."
                    searchPlaceholder="بحث عن حساب بالرمز أو الاسم..."
                    emptyMessage="لا توجد نتائج"
                  />
                </div>
                <Input
                  label="مدين"
                  type="number"
                  value={editingLine.debit || ''}
                  onChange={(event) => setEditingLine({ ...editingLine, debit: Number(event.target.value), credit: 0 })}
                />
                <Input
                  label="دائن"
                  type="number"
                  value={editingLine.credit || ''}
                  onChange={(event) => setEditingLine({ ...editingLine, credit: Number(event.target.value), debit: 0 })}
                />
                <Select
                  label="مركز التكلفة"
                  value={editingLine.costCenterId || ''}
                  onChange={(event) => setEditingLine({ ...editingLine, costCenterId: event.target.value ? Number(event.target.value) : null })}
                  options={[
                    { value: '', label: 'بدون' },
                    ...costCenters.map((costCenter) => ({ value: String(costCenter.id), label: `${costCenter.code} - ${costCenter.name}` })),
                  ]}
                />
                <Input
                  label="البيان"
                  type="text"
                  value={editingLine.description ?? ''}
                  onChange={(event) => setEditingLine({ ...editingLine, description: event.target.value })}
                  placeholder="بيان السطر..."
                />
              </div>
              <div className="flex justify-end gap-2 mt-3">
                <Button type="button" variant="outline" size="sm" onClick={() => setEditingLine(null)}>إلغاء</Button>
                <Button type="button" variant="primary" size="sm" onClick={handleSaveLine}>حفظ</Button>
              </div>
            </div>
          )}

          <div className="mt-3">
            <BalanceIndicator totalDebit={totalDebit} totalCredit={totalCredit} />
          </div>
        </div>
      </Card>
    </form>
  );
}
