import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useCreateJournalEntry } from '@/features/accounting/hooks/useJournalEntries';
import { useCreateJournalEntryLine } from '@/features/accounting/hooks/useJournalEntryLines';
import { useFiscalYearByDate } from '@/features/accounting/hooks/useFiscalYearByDate';
import { useJournalsList } from '@/features/accounting/hooks/useJournalsList';
import { useCurrenciesList } from '@/features/accounting/hooks/useCurrenciesList';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCostCenters } from '@/features/organization/hooks/useCostCenters';
import { FiscalYearIndicator } from '@/components/AccountingFiscalYearIndicator';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';
import { MoveEntryType } from '../web-api-client';
import { notify } from '@/features/notifications/notify';
import { Button, Card, Combobox, Input, Select, Textarea, Badge } from '@/components/ui';

const headerSchema = z.object({
  documentDate: z.string().min(1, 'التاريخ مطلوب'),
  journalId: z.number().min(1, 'اختر يومية'),
  baseCurrencyId: z.number().min(1, 'اختر العملة'),
  narration: z.string().optional().nullable(),
  ref: z.string().optional().nullable(),
  entryType: z.string().min(1, 'اختر النوع'),
});

type HeaderFormData = z.infer<typeof headerSchema>;

const entryTypeLabels: Record<MoveEntryType, string> = {
  [MoveEntryType.Standard]: 'قيود عامة',
  [MoveEntryType.Reversing]: 'قيود عكسية',
  [MoveEntryType.Adjusting]: 'قيود تسوية',
  [MoveEntryType.Opening]: 'قيد افتتاحي',
  [MoveEntryType.Closing]: 'قيد إغلاق',
  [MoveEntryType.SystemGenerated]: 'مولد آلياً',
};

const manualEntryTypes: MoveEntryType[] = [
  MoveEntryType.Standard,
  MoveEntryType.Reversing,
  MoveEntryType.Adjusting,
  MoveEntryType.Opening,
];

interface EntryLine {
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

interface AccountingJournalEntryFormProps {
  onSuccess?: (entryId: number) => void;
  onCancel?: () => void;
  onStateChange?: (state: { canSave: boolean; isSaving: boolean }) => void;
}

export function AccountingJournalEntryForm({ onSuccess, onStateChange }: AccountingJournalEntryFormProps) {
  const createEntry = useCreateJournalEntry();
  const createLine = useCreateJournalEntryLine();
  const [lines, setLines] = useState<EntryLine[]>([]);
  const [editingLine, setEditingLine] = useState<Partial<EntryLine> | null>(null);

  const { data: journals } = useJournalsList({ isActive: true });
  const { data: currencies } = useCurrenciesList();
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const { data: costCenters = [] } = useCostCenters();

  const accountMap = new Map(accounts.map((a) => [a.id, `${a.code} - ${a.name}`]));
  const costCenterMap = new Map(costCenters.map((c) => [c.id, `${c.code} - ${c.name}`]));

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    getValues,
    formState: { errors },
  } = useForm<HeaderFormData>({
    resolver: zodResolver(headerSchema),
    defaultValues: { documentDate: new Date().toISOString().split('T')[0] },
  });

  const watchedDate = watch('documentDate');
  const fiscalData = useFiscalYearByDate(watchedDate);

  const baseCurrency = currencies?.find((c) => c.isBase);

  useEffect(() => {
    if (baseCurrency?.id && !getValues('baseCurrencyId')) {
      setValue('baseCurrencyId', baseCurrency.id, { shouldValidate: true });
    }
  }, [baseCurrency, getValues, setValue]);

  const totalDebit = lines.reduce((s, l) => s + (l.debit || 0), 0);
  const totalCredit = lines.reduce((s, l) => s + (l.credit || 0), 0);
  const isBalanced = Math.abs(totalDebit - totalCredit) < 0.001;
  const canSave = lines.length > 0 && isBalanced;
  const isSaving = createEntry.isPending || createLine.isPending;

  useEffect(() => {
    onStateChange?.({ canSave, isSaving });
  }, [canSave, isSaving, onStateChange]);

  const handleAddLine = () => setEditingLine({
    id: 0, journalEntryId: 0, sequence: lines.length + 1, accountId: 0,
    description: null, currencyId: 1, exchangeRate: 1,
    debit: 0, credit: 0, costCenterId: null,
    paymentOrderId: null, rowVersion: '',
  });

  const handleSaveLine = () => {
    if (!editingLine) return;
    if (editingLine.debit && editingLine.credit) { notify({ type: 'error', title: 'يجب أن يكون البند مدين أو دائن فقط' }); return; }
    if (!editingLine.debit && !editingLine.credit) { notify({ type: 'error', title: 'يجب إدخال مبلغ مدين أو دائن' }); return; }
    if (!editingLine.accountId) { notify({ type: 'error', title: 'يجب اختيار الحساب' }); return; }
    setLines([...lines, editingLine as EntryLine]);
    setEditingLine(null);
  };

  const handleRemoveLine = (index: number) => setLines(lines.filter((_, i) => i !== index));

  const onSubmit = async (data: HeaderFormData) => {
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
          accountId: line.accountId, description: line.description,
          currencyId: line.currencyId, exchangeRate: line.exchangeRate,
          debit: line.debit, credit: line.credit,
          costCenterId: line.costCenterId,
        },
      });
    }
    onSuccess?.(newEntryId);
  };

  return (
    <form id="journal-entry-form" onSubmit={handleSubmit(onSubmit)}>
      <Card variant="default">
        <div className="px-4 py-2.5 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)] flex items-center justify-between">
          <h2 className="text-sm font-bold text-[var(--color-on-surface)]">بيانات القيد</h2>
          {watchedDate && <FiscalYearIndicator date={watchedDate} />}
        </div>
        <div className="p-4">
          <div className="grid grid-cols-5 gap-3">
            <Input
              id="documentDate"
              type="date"
              label="التاريخ"
              {...register('documentDate')}
              error={errors.documentDate?.message}
              required
            />
            <Select
              id="journalId"
              label="اليومية"
              {...register('journalId', { valueAsNumber: true })}
              options={[
                { value: '', label: 'اختر يومية...' },
                ...(journals?.map((j) => ({ value: String(j.id), label: `${j.code} - ${j.name}` })) ?? []),
              ]}
              error={errors.journalId?.message}
            />
            <Select
              id="entryType"
              label="نوع القيد"
              {...register('entryType')}
              options={[
                { value: '', label: 'اختر النوع...' },
                ...manualEntryTypes.map((t) => ({ value: t, label: entryTypeLabels[t] })),
              ]}
              error={errors.entryType?.message}
            />
            <Select
              id="baseCurrencyId"
              label="العملة"
              {...register('baseCurrencyId', { valueAsNumber: true })}
              options={[
                { value: '', label: 'اختر العملة...' },
                ...(currencies?.map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` })) ?? []),
              ]}
              error={errors.baseCurrencyId?.message}
            />
            <Input
              id="ref"
              type="text"
              label="المرجع"
              placeholder="رقم المرجع..."
              {...register('ref')}
            />
          </div>
          <div className="mt-3">
            <Textarea
              id="narration"
              label="البيان"
              rows={1}
              className="min-h-0"
              placeholder="بيان القيد..."
              {...register('narration')}
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
              <Button type="button" variant="secondary" size="sm" onClick={handleAddLine}>
                + إضافة بيان
              </Button>
            </div>
          </div>
        </div>
        <div className="p-4">
          {lines.length === 0 && !editingLine ? (
            <div className="text-center py-8 rounded-lg border-2 border-dashed border-[var(--color-outline-variant)]">
              <p className="text-sm text-[var(--color-on-surface-variant)]">لا يوجد بيان بعد</p>
            </div>
          ) : (
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
                    <th className="px-3 py-2" />
                  </tr>
                </thead>
                <tbody className="divide-y border-[var(--color-outline-variant)]">
                  {lines.map((line, index) => (
                    <tr key={index} className="bg-[var(--color-surface)]">
                      <td className="px-3 py-2 text-xs text-[var(--color-on-surface-variant)]">{line.sequence}</td>
                      <td className="px-3 py-2 text-xs font-bold text-[var(--color-on-surface)]">{accountMap.get(line.accountId) || line.accountId}</td>
                      <td className="px-3 py-2 text-xs text-[var(--color-on-surface-variant)]">{costCenterMap.get(line.costCenterId ?? 0) || '-'}</td>
                      <td className="px-3 py-2 text-xs text-start tabular-nums font-bold text-[var(--color-on-surface)]">{line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}</td>
                      <td className="px-3 py-2 text-xs text-start tabular-nums font-bold text-[var(--color-on-surface)]">{line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}</td>
                      <td className="px-3 py-2 text-xs text-[var(--color-on-surface-variant)]">{line.description || '-'}</td>
                      <td className="px-3 py-2 text-xs">
                        <Button type="button" variant="ghost" size="sm" onClick={() => handleRemoveLine(index)} className="text-[var(--color-error)] hover:text-[var(--color-error)]">حذف</Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {editingLine && (
            <div className="mt-3 rounded-lg border p-3 bg-[var(--color-surface-container-low)] border-[var(--color-outline-variant)]">
              <div className="grid grid-cols-6 gap-3">
                <div className="col-span-2">
                  <Combobox
                    label="الحساب"
                    value={editingLine.accountId ? String(editingLine.accountId) : ''}
                    onChange={(val) => setEditingLine({ ...editingLine, accountId: val ? Number(val) : 0 })}
                    options={accounts.map((a) => ({ value: String(a.id), label: `${a.code} - ${a.name}` }))}
                    placeholder="اختر الحساب..."
                    searchPlaceholder="بحث عن حساب بالرمز أو الاسم..."
                    emptyMessage="لا توجد نتائج"
                  />
                </div>
                <Input
                  label="مدين"
                  type="number"
                  value={editingLine.debit || ''}
                  onChange={(e) => setEditingLine({ ...editingLine, debit: Number(e.target.value), credit: 0 })}
                />
                <Input
                  label="دائن"
                  type="number"
                  value={editingLine.credit || ''}
                  onChange={(e) => setEditingLine({ ...editingLine, credit: Number(e.target.value), debit: 0 })}
                />
                <Select
                  label="مركز التكلفة"
                  value={editingLine.costCenterId || ''}
                  onChange={(e) => setEditingLine({ ...editingLine, costCenterId: e.target.value ? Number(e.target.value) : null })}
                  options={[
                    { value: '', label: 'بدون' },
                    ...costCenters.map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` })),
                  ]}
                />
                <Input
                  label="البيان"
                  type="text"
                  value={editingLine.description || ''}
                  onChange={(e) => setEditingLine({ ...editingLine, description: e.target.value })}
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
