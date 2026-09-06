import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { useCreateMove } from '../hooks/useMoves';
import { useCreateMoveLine } from '../hooks/useMoveLines';
import { useAccountsList } from '../hooks/useAccountsList';
import { useJournalsList } from '../hooks/useJournalsList';
import { useCurrenciesList } from '../hooks/useCurrenciesList';
import { useCostCenters } from '../../organization/hooks/useCostCenters';
import { useExchangeRateLookup } from '../hooks/useExchangeRateLookup';
import { FiscalYearIndicator } from '../components/FiscalYearIndicator';
import { BalanceIndicator } from '../components/BalanceIndicator';
import { useFiscalYearByDate } from '../hooks/useFiscalYearByDate';
import { showToast } from '@/components/ui/Toast';
import type { CreateMoveCommand, CreateMoveLineCommand } from '../types';

const moveHeaderSchema = z.object({
  documentDate: z.string().min(1, 'تاريخ المستند مطلوب'),
  journalId: z.string().optional(),
  entryType: z.string().optional(),
  ref: z.string().max(100, 'المرجع لا يتجاوز 100 حرف').optional(),
  narration: z.string().max(1000, 'البيان لا يتجاوز 1000 حرف').optional(),
});

type MoveHeaderForm = z.infer<typeof moveHeaderSchema>;

interface PendingLine {
  key: number;
  data: CreateMoveLineCommand;
}

export function MoveCreatePage() {
  const lineKeyRef = useRef(0);
  const navigate = useNavigate();
  const createMove = useCreateMove();
  const createLine = useCreateMoveLine();
  const [submitting, setSubmitting] = useState(false);

  const { data: accountsData } = useAccountsList({ isPostable: true, isActive: true });
  const { data: currenciesData } = useCurrenciesList({ isActive: true });
  const { data: costCentersData } = useCostCenters();
  const { data: journalsData } = useJournalsList({ isActive: true });

  const accounts = accountsData ?? [];
  const currencies = currenciesData ?? [];
  const costCenters = costCentersData ?? [];
  const journals = journalsData ?? [];

  const headerForm = useForm<MoveHeaderForm>({
    resolver: zodResolver(moveHeaderSchema),
    defaultValues: {
      documentDate: new Date().toISOString().slice(0, 10),
      journalId: '',
      entryType: '',
      ref: '',
      narration: '',
    },
  });

  const watchDocumentDate = headerForm.watch('documentDate');
  const { data: fiscalInfo } = useFiscalYearByDate(watchDocumentDate);

  const [lines, setLines] = useState<PendingLine[]>([]);

  const [lineForm, setLineForm] = useState({
    accountId: '',
    description: '',
    currencyId: '1',
    exchangeRate: '1',
    debit: '',
    credit: '',
    costCenterId: '',
  });

  const baseCurrencyId = 1;
  const selectedCurrencyId = lineForm.currencyId ? Number(lineForm.currencyId) : null;
  const { data: exchangeRate } = useExchangeRateLookup(
    baseCurrencyId,
    selectedCurrencyId,
    watchDocumentDate
  );

  const handleCurrencyChange = (value: string) => {
    setLineForm((prev) => ({ ...prev, currencyId: value }));
    if (value && value !== '1' && exchangeRate?.rate) {
      setLineForm((prev) => ({ ...prev, exchangeRate: exchangeRate.rate.toString() }));
    }
  };

  const addLine = () => {
    const debit = parseFloat(lineForm.debit) || 0;
    const credit = parseFloat(lineForm.credit) || 0;
    if (!lineForm.accountId || (debit === 0 && credit === 0)) return;

    setLines((prev) => [
      ...prev,
      {
        key: lineKeyRef.current++,
        data: {
          accountId: Number(lineForm.accountId),
          description: lineForm.description || null,
          currencyId: Number(lineForm.currencyId),
          exchangeRate: Number(lineForm.exchangeRate),
          debit,
          credit,
          costCenterId: lineForm.costCenterId ? Number(lineForm.costCenterId) : null,
        },
      },
    ]);
    setLineForm({
      accountId: '', description: '', currencyId: '1', exchangeRate: '1',
      debit: '', credit: '', costCenterId: '',
    });
  };

  const removeLine = (key: number) => {
    setLines((prev) => prev.filter((l) => l.key !== key));
  };

  const handleSubmit = headerForm.handleSubmit(async (values) => {
    if (!fiscalInfo?.fiscalYearId || !fiscalInfo?.fiscalPeriodId) {
      showToast('error', 'لا توجد سنة مالية أو فترة مفتوحة لهذا التاريخ');
      return;
    }
    if (lines.length === 0) {
      showToast('error', 'يجب إضافة سطر واحد على الأقل');
      return;
    }
    setSubmitting(true);
    try {
      const result = await createMove.mutateAsync({
        documentDate: values.documentDate,
        journalId: values.journalId ? Number(values.journalId) : null,
        periodId: fiscalInfo.fiscalPeriodId,
        fiscalYearId: fiscalInfo.fiscalYearId,
        narration: values.narration || null,
        ref: values.ref || null,
        entryType: values.entryType ? Number(values.entryType) : null,
      } as CreateMoveCommand);

      const moveId = (result as { id: number })?.id;
      if (!moveId) throw new Error('فشل إنشاء القيد');

      for (const line of lines) {
        await createLine.mutateAsync({ moveId, data: line.data });
      }

      showToast('success', `تم إنشاء القيد بـ ${lines.length} سطر`);
      navigate(`/accounting/journal-entries/${moveId}`);
    } catch (e) {
      showToast('error', (e as Error).message || 'فشل إنشاء القيد');
    } finally {
      setSubmitting(false);
    }
  });

  const canSubmit = headerForm.formState.isValid && lines.length > 0 && !!fiscalInfo && !submitting;

  return (
    <div className="space-y-2">
      <PageHeader title="إنشاء قيد يومي"  />

      <FiscalYearIndicator date={watchDocumentDate} />

      {/* ═══ رأس القيد ═══ */}
      <form onSubmit={handleSubmit} className="bg-[var(--color-surface-container-lowest)] border border-[var(--color-primary-container)] rounded-xl p-4 space-y-4" aria-label="إنشاء قيد يومي">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Input
            id="cd"
            label="تاريخ المستند *"
            type="date"
            {...headerForm.register('documentDate')}
          />
          <Select
            id="jid"
            label="اليومية"
            {...headerForm.register('journalId')}
            options={[
              { value: '', label: 'اختر اليومية' },
              ...journals.map((j) => ({ value: String(j.id), label: `${j.code} — ${j.name}` })),
            ]}
          />
          <Select
            id="et"
            label="نوع القيد"
            {...headerForm.register('entryType')}
            options={[
              { value: '', label: 'اختر النوع' },
              { value: '0', label: 'عادي' },
              { value: '1', label: 'عكس' },
              { value: '2', label: 'تسوية' },
              { value: '3', label: 'إقفال' },
              { value: '4', label: 'افتتاح' },
            ]}
          />
          <Input
            id="ref"
            label="المرجع"
            type="text"
            {...headerForm.register('ref')}
          />
        </div>

        {headerForm.formState.errors.ref && (
          <p className="text-xs text-error">{headerForm.formState.errors.ref.message}</p>
        )}

        <div>
          <label htmlFor="narr" className="block text-xs font-semibold text-on-surface-variant mb-1">البيان</label>
          <textarea
            id="narr"
            rows={1}
            {...headerForm.register('narration')}
            className="w-full px-3 py-2 border-2 border-[var(--color-border-input)] rounded-lg bg-[var(--color-surface-container-lowest)] text-sm focus-visible:outline-2 focus-visible:outline-secondary"
          />
          {headerForm.formState.errors.narration && (
            <p className="text-xs text-error">{headerForm.formState.errors.narration.message}</p>
          )}
        </div>

        {/* ═══ السطور ═══ */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold text-on-surface">السطور</h3>

          {lines.length > 0 && (
            <div className="overflow-x-auto border border-[var(--color-primary-container)] rounded-xl">
              <table role="table" aria-label="سطور القيد" className="w-full border-collapse text-sm leading-relaxed">
                <thead>
                  <tr>
                    <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-start">م</th>
                    <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-start">الحساب</th>
                    <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-start">البيان</th>
                    <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-end tabular-nums">مدين</th>
                    <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap text-end tabular-nums">دائن</th>
                    <th className="px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-white bg-[var(--color-primary)] border-b border-[var(--color-primary-container)] whitespace-nowrap text-center">إجراء</th>
                  </tr>
                </thead>
                <tbody>
                  {lines.map((l, i) => (
                    <tr key={l.key} className={`${i % 2 === 1 ? 'bg-[rgba(0,32,69,0.06)]' : 'bg-[var(--color-surface-container-lowest)]'} transition-all duration-150 hover:bg-[color-mix(in_srgb,var(--color-primary-container)_5%,transparent)]`}>
                      <td className="px-3 py-1.5 tabular-nums">{i + 1}</td>
                      <td className="px-3 py-1.5">
                        {(() => { const a = accounts.find((x) => x.id === l.data.accountId); return a ? <><span className="font-medium">{a.code}</span> — {a.name}</> : l.data.accountId; })()}
                      </td>
                      <td className="px-3 py-1.5">{l.data.description ?? '—'}</td>
                      <td className="px-3 py-1.5 text-end tabular-nums">{l.data.debit.toFixed(2)}</td>
                      <td className="px-3 py-1.5 text-end tabular-nums">{l.data.credit.toFixed(2)}</td>
                      <td className="px-3 py-1.5 text-center">
                        <Button variant="ghost" size="sm" onClick={() => removeLine(l.key)}
                          aria-label={`حذف سطر ${i + 1}`}
                          className="text-error">
                          حذف
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          <div className="border border-[var(--color-primary-container)] rounded-xl p-4 bg-[var(--color-surface-container-lowest)] space-y-3">
          
            <div className="grid grid-cols-2 md:grid-cols-6 gap-3">
              <Select
                label="الحساب *"
                value={lineForm.accountId}
                onChange={(e) => setLineForm({ ...lineForm, accountId: e.target.value })}
                options={[
                  { value: '', label: 'اختر الحساب' },
                  ...accounts.map((a) => ({ value: String(a.id), label: `${a.code} — ${a.name}` })),
                ]}
              />
              <Input
                label="مدين"
                type="number"
                step="0.01"
                value={lineForm.debit}
                onChange={(e) => setLineForm({ ...lineForm, debit: e.target.value })}
              />
              <Input
                label="دائن"
                type="number"
                step="0.01"
                value={lineForm.credit}
                onChange={(e) => setLineForm({ ...lineForm, credit: e.target.value })}
              />
              <Select
                label="العملة"
                value={lineForm.currencyId}
                onChange={(e) => handleCurrencyChange(e.target.value)}
                options={[
                  { value: '', label: 'اختر العملة' },
                  ...currencies.map((c) => ({ value: String(c.id), label: `${c.code} — ${c.name}` })),
                ]}
              />
              <Select
                label="مركز التكلفة"
                value={lineForm.costCenterId}
                onChange={(e) => setLineForm({ ...lineForm, costCenterId: e.target.value })}
                options={[
                  { value: '', label: 'بدون' },
                  ...costCenters.map((cc) => ({ value: String(cc.id), label: `${cc.code} — ${cc.name}` })),
                ]}
              />
              <Input
                label="البيان"
                type="text"
                value={lineForm.description}
                onChange={(e) => setLineForm({ ...lineForm, description: e.target.value })}
              />
            </div>
            <div className="flex justify-end">
              <Button variant="primary" size="sm" onClick={addLine}>
                إضافة سطر
              </Button>
            </div>
          </div>

          {lines.length > 0 && <BalanceIndicator lines={lines} />}
        </div>

        <div className="flex justify-between items-center pt-3">
          <Button variant="ghost" onClick={() => navigate('/accounting/journal-entries')} className="text-sm">
            إلغاء
          </Button>
          <Button type="submit" variant="primary" disabled={!canSubmit} loading={submitting} className="text-sm">
            {submitting ? 'جاري الإنشاء...' : `إنشاء القيد (${lines.length} سطر)`}
          </Button>
        </div>
      </form>
    </div>
  );
}
