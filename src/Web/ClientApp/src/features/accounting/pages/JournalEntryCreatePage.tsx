import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useCreateJournalEntry } from '../hooks/useJournalEntries';
import { useCreateJournalEntryLine } from '../hooks/useJournalEntryLines';
import { useFiscalYearByDate } from '../hooks/useFiscalYearByDate';
import { useJournalsList } from '../hooks/useJournalsList';
import { useCurrenciesList } from '../hooks/useCurrenciesList';
import { EntryLinesEditor, type EntryLine } from '@/components/AccountingEntryLinesEditor';
import { FiscalYearIndicator } from '@/components/AccountingFiscalYearIndicator';
import { MoveEntryType } from '../../../web-api-client';
import { notify } from '@/features/notifications/notify';
import { Page, Button, Card, Input, Select, Textarea } from '@/components/ui';

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

export function JournalEntryCreatePage() {
  const navigate = useNavigate();
  const createEntry = useCreateJournalEntry();
  const createLine = useCreateJournalEntryLine();
  const [lines, setLines] = useState<EntryLine[]>([]);

  const { data: journals } = useJournalsList({ isActive: true });
  const { data: currencies } = useCurrenciesList();

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<HeaderFormData>({
    resolver: zodResolver(headerSchema),
    defaultValues: { documentDate: new Date().toISOString().split('T')[0] },
  });

  const watchedDate = watch('documentDate');
  const fiscalData = useFiscalYearByDate(watchedDate);

  const totalDebit = lines.reduce((s, l) => s + (l.debit || 0), 0);
  const totalCredit = lines.reduce((s, l) => s + (l.credit || 0), 0);
  const isBalanced = Math.abs(totalDebit - totalCredit) < 0.001;
  const canSave = lines.length > 0 && isBalanced;
  const isSaving = createEntry.isPending || createLine.isPending;

  const handleAddLine = (line: EntryLine) => setLines([...lines, line]);
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
    navigate(`/accounting/journal-entries/${newEntryId}`);
  };

  return (
    <Page title="إنشاء قيد يومية" description="أدخل بيانات القيد ثم أضف الأسطر" maxWidth="lg">
      <form id="journal-entry-form" onSubmit={handleSubmit(onSubmit)}>
        <div className="space-y-6">
          <Card variant="default">
            <div className="px-6 py-4 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
              <h2 className="text-base font-bold text-[var(--color-on-surface)]">بيانات القيد</h2>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-2 gap-5">
                <Input
                  id="documentDate"
                  type="date"
                  label="التاريخ"
                  {...register('documentDate')}
                  error={errors.documentDate?.message}
                  required
                />
                {watchedDate && <FiscalYearIndicator date={watchedDate} />}
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
              </div>
              <div className="mt-5 grid grid-cols-2 gap-5">
                <Input
                  id="ref"
                  type="text"
                  label="المرجع"
                  placeholder="رقم المرجع..."
                  {...register('ref')}
                />
                <Textarea
                  id="narration"
                  label="الوصف"
                  rows={2}
                  placeholder="وصف القيد..."
                  {...register('narration')}
                />
              </div>
            </div>
          </Card>

          <Card variant="default">
            <div className="px-6 py-4 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
              <div className="flex items-center justify-between">
                <h2 className="text-base font-bold text-[var(--color-on-surface)]">أسطر القيد</h2>
                {lines.length > 0 && (
                  <span className="text-xs px-2 py-0.5 rounded-full font-bold bg-[var(--color-surface-container-high)] text-[var(--color-on-surface-variant)]">
                    {lines.length} أسطر
                  </span>
                )}
              </div>
            </div>
            <div className="p-6">
              <EntryLinesEditor lines={lines} onAdd={handleAddLine} onRemove={handleRemoveLine} />
            </div>
          </Card>
        </div>
      </form>

      <div className="mt-8 flex items-center gap-3">
        <Button type="submit" form="journal-entry-form" variant="primary" disabled={!canSave || isSaving} loading={isSaving}>
          حفظ القيد
        </Button>
        <Button variant="ghost" onClick={() => navigate('/accounting/journal-entries')} disabled={isSaving}>
          إلغاء
        </Button>
      </div>
    </Page>
  );
}
