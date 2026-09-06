import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useCreateJournalEntry } from '../hooks/useJournalEntries';
import { useCreateJournalEntryLine } from '../hooks/useJournalEntryLines';
import { useFiscalYearByDate } from '../hooks/useFiscalYearByDate';
import { useJournalsList } from '../hooks/useJournalsList';
import { useCurrenciesList } from '../hooks/useCurrenciesList';
import { useAccountsList } from '../hooks/useAccountsList';
import { BalanceIndicator } from '../components/BalanceIndicator';
import { FiscalYearIndicator } from '../components/FiscalYearIndicator';
import { DimensionPickers, type DimensionValues } from '../components/DimensionPickers';
import type { JournalEntryLineDto } from '../types';

const headerSchema = z.object({
  documentDate: z.string().min(1, 'التاريخ مطلوب'),
  journalId: z.number().min(1, 'اختر يومية'),
  baseCurrencyId: z.number().min(1, 'اختر العملة'),
  narration: z.string().optional().nullable(),
  ref: z.string().optional().nullable(),
  entryType: z.string().min(1, 'اختر النوع'),
});

type HeaderFormData = z.infer<typeof headerSchema>;

export function JournalEntryCreatePage() {
  const navigate = useNavigate();
  const createEntry = useCreateJournalEntry();
  const createLine = useCreateJournalEntryLine();
  const [lines, setLines] = useState<JournalEntryLineDto[]>([]);
  const [editingLine, setEditingLine] = useState<Partial<JournalEntryLineDto> | null>(null);

  const { data: journals } = useJournalsList({ isActive: true });
  const { data: currencies } = useCurrenciesList();
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });

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

  const totalDebit = useMemo(() => lines.reduce((sum, l) => sum + (l.debit || 0), 0), [lines]);
  const totalCredit = useMemo(() => lines.reduce((sum, l) => sum + (l.credit || 0), 0), [lines]);
  const isBalanced = Math.abs(totalDebit - totalCredit) < 0.001;
  const canSave = lines.length > 0 && isBalanced;
  const isSaving = createEntry.isPending || createLine.isPending;

  const handleAddLine = () => {
    setEditingLine({
      id: 0,
      journalEntryId: 0,
      sequence: lines.length + 1,
      accountId: 0,
      accountCode: '',
      accountName: '',
      currencyId: 1,
      exchangeRate: 1,
      debit: 0,
      credit: 0,
      fundId: null,
      projectId: null,
      budgetItemId: null,
      encumbranceId: null,
      paymentOrderId: null,
      rowVersion: '',
    });
  };

  const handleSaveLine = () => {
    if (!editingLine) return;
    if (editingLine.debit && editingLine.credit) { alert('يجب أن يكون السطر مدين أو دائن فقط'); return; }
    if (!editingLine.debit && !editingLine.credit) { alert('يجب إدخال مبلغ مدين أو دائن'); return; }
    if (!editingLine.accountId) { alert('يجب اختيار الحساب'); return; }
    setLines([...lines, editingLine as JournalEntryLineDto]);
    setEditingLine(null);
  };

  const handleRemoveLine = (index: number) => {
    setLines(lines.filter((_, i) => i !== index));
  };

  const onSubmit = async (data: HeaderFormData) => {
    if (!fiscalData.data) { alert('لا توجد سنة مالية مفتوحة لهذا التاريخ'); return; }

    const result = await createEntry.mutateAsync({
      documentDate: data.documentDate,
      journalId: data.journalId,
      periodId: fiscalData.data.fiscalPeriodId,
      fiscalYearId: fiscalData.data.fiscalYearId,
      baseCurrencyId: data.baseCurrencyId,
      narration: data.narration,
      ref: data.ref,
      entryType: data.entryType,
    });

    const newEntryId = result as unknown as number;
    for (const line of lines) {
      await createLine.mutateAsync({
        journalEntryId: newEntryId,
        command: {
          accountId: line.accountId, description: line.description,
          currencyId: line.currencyId, exchangeRate: line.exchangeRate,
          debit: line.debit, credit: line.credit,
          costCenterId: line.costCenterId,
          fundId: line.fundId,
          projectId: line.projectId,
          budgetItemId: line.budgetItemId,
          encumbranceId: line.encumbranceId,
          paymentOrderId: line.paymentOrderId,
        },
      });
    }
    navigate(`/accounting/journal-entries/${newEntryId}`);
  };

  const cardClass = 'rounded-xl border overflow-hidden';
  const cardBorder = { borderColor: 'var(--color-outlineVariant)' };
  const cardHeader = 'px-6 py-4 border-b';
  const inputClass = 'w-full px-3 py-2 rounded-lg border text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-focus-ring)] focus:border-[var(--color-focus-ring)] transition-colors duration-200';
  const labelClass = 'block text-sm font-bold mb-1.5';
  const errorClass = 'mt-1 text-xs font-medium';

  return (
    <div className="max-w-6xl mx-auto py-8 px-6" dir="rtl">
      <div className="mb-8">
        <h1 className="text-2xl font-bold tracking-tight" style={{ color: 'var(--color-onSurface)' }}>إنشاء قيد يومية</h1>
        <p className="mt-1 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>أدخل بيانات القيد ثم أضف الأسطر</p>
      </div>

      <form id="journal-entry-form" onSubmit={handleSubmit(onSubmit)}>
        <div className="space-y-6">
          {/* بيانات القيد */}
          <div className={cardClass} style={{ backgroundColor: 'var(--color-surface)', ...cardBorder }}>
            <div className={cardHeader} style={{ borderColor: 'var(--color-outlineVariant)', backgroundColor: 'var(--color-surfaceContainerLow)' }}>
              <h2 className="text-base font-bold" style={{ color: 'var(--color-onSurface)' }}>بيانات القيد</h2>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-2 gap-5">
                <div>
                  <label htmlFor="documentDate" className={labelClass} style={{ color: 'var(--color-onSurface)' }}>التاريخ</label>
                  <input id="documentDate" type="date" {...register('documentDate')} className={inputClass}
                    style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: errors.documentDate ? 'var(--color-error)' : 'var(--color-outlineVariant)' }} />
                  {errors.documentDate && <p className={errorClass} style={{ color: 'var(--color-error)' }}>{errors.documentDate.message}</p>}
                  {watchedDate && <FiscalYearIndicator date={watchedDate} />}
                </div>
                <div>
                  <label htmlFor="journalId" className={labelClass} style={{ color: 'var(--color-onSurface)' }}>اليومية</label>
                  <select id="journalId" {...register('journalId', { valueAsNumber: true })} className={inputClass}
                    style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: errors.journalId ? 'var(--color-error)' : 'var(--color-outlineVariant)' }}>
                    <option value="">اختر يومية...</option>
                    {journals?.map((j) => <option key={j.id} value={j.id}>{j.code} - {j.name}</option>)}
                  </select>
                  {errors.journalId && <p className={errorClass} style={{ color: 'var(--color-error)' }}>{errors.journalId.message}</p>}
                </div>
                <div>
                  <label htmlFor="entryType" className={labelClass} style={{ color: 'var(--color-onSurface)' }}>نوع القيد</label>
                  <select id="entryType" {...register('entryType')} className={inputClass}
                    style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: errors.entryType ? 'var(--color-error)' : 'var(--color-outlineVariant)' }}>
                    <option value="">اختر النوع...</option>
                    <option value="Standard">قيود عامة</option>
                    <option value="Reversing">قيود عكسية</option>
                    <option value="Adjusting">قيود تسوية</option>
                  </select>
                  {errors.entryType && <p className={errorClass} style={{ color: 'var(--color-error)' }}>{errors.entryType.message}</p>}
                </div>
                <div>
                  <label htmlFor="baseCurrencyId" className={labelClass} style={{ color: 'var(--color-onSurface)' }}>العملة</label>
                  <select id="baseCurrencyId" {...register('baseCurrencyId', { valueAsNumber: true })} className={inputClass}
                    style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: errors.baseCurrencyId ? 'var(--color-error)' : 'var(--color-outlineVariant)' }}>
                    <option value="">اختر العملة...</option>
                    {currencies?.map((c) => <option key={c.id} value={c.id}>{c.code} - {c.name}</option>)}
                  </select>
                  {errors.baseCurrencyId && <p className={errorClass} style={{ color: 'var(--color-error)' }}>{errors.baseCurrencyId.message}</p>}
                </div>
              </div>
              <div className="mt-5 grid grid-cols-2 gap-5">
                <div>
                  <label htmlFor="ref" className={labelClass} style={{ color: 'var(--color-onSurface)' }}>المرجع</label>
                  <input id="ref" type="text" {...register('ref')} placeholder="رقم المرجع..." className={inputClass}
                    style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
                </div>
                <div>
                  <label htmlFor="narration" className={labelClass} style={{ color: 'var(--color-onSurface)' }}>الوصف</label>
                  <textarea id="narration" {...register('narration')} rows={2} placeholder="وصف القيد..." className={inputClass}
                    style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
                </div>
              </div>
            </div>
          </div>

          {/* أسطر القيد */}
          <div className={cardClass} style={{ backgroundColor: 'var(--color-surface)', ...cardBorder }}>
            <div className={cardHeader} style={{ borderColor: 'var(--color-outlineVariant)', backgroundColor: 'var(--color-surfaceContainerLow)' }}>
              <div className="flex items-center justify-between">
                <h2 className="text-base font-bold" style={{ color: 'var(--color-onSurface)' }}>أسطر القيد</h2>
                {lines.length > 0 && (
                  <span className="text-xs px-2 py-0.5 rounded-full font-bold" style={{ backgroundColor: 'var(--color-surfaceContainerHigh)', color: 'var(--color-onSurfaceVariant)' }}>
                    {lines.length} أسطر
                  </span>
                )}
              </div>
            </div>
            <div className="p-6">
              <div className="flex justify-end mb-4">
                <button type="button" onClick={handleAddLine}
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
                        <th className="px-4 py-3 text-left text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>مدين</th>
                        <th className="px-4 py-3 text-left text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>دائن</th>
                        <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>الوصف</th>
                        <th className="px-4 py-3 text-right text-xs font-bold" style={{ color: 'var(--color-onSurfaceVariant)' }}>الأبعاد</th>
                        <th className="px-4 py-3" />
                      </tr>
                    </thead>
                    <tbody className="divide-y" style={{ borderColor: 'var(--color-outlineVariant)' }}>
                      {lines.map((line, index) => (
                        <tr key={index} style={{ backgroundColor: 'var(--color-surface)' }}>
                          <td className="px-4 py-3 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>{line.sequence}</td>
                          <td className="px-4 py-3 text-sm font-bold" style={{ color: 'var(--color-onSurface)' }}>{line.accountCode} - {line.accountName}</td>
                          <td className="px-4 py-3 text-sm text-left tabular-nums font-bold" style={{ color: 'var(--color-onSurface)' }}>{line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}</td>
                          <td className="px-4 py-3 text-sm text-left tabular-nums font-bold" style={{ color: 'var(--color-onSurface)' }}>{line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}</td>
                          <td className="px-4 py-3 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>{line.description || '-'}</td>
                          <td className="px-4 py-3 text-xs" style={{ color: 'var(--color-onSurfaceVariant)' }}>
                            {[
                              line.fundName && `صندوق: ${line.fundName}`,
                              line.projectName && `مشروع: ${line.projectName}`,
                              line.budgetItemCode && `بند: ${line.budgetItemCode}`,
                              line.encumbranceNumber && `التزام: ${line.encumbranceNumber}`,
                              line.paymentOrderNumber && `دفع: ${line.paymentOrderNumber}`,
                            ].filter(Boolean).join(' · ') || '-'}
                          </td>
                          <td className="px-4 py-3 text-sm">
                            <button type="button" onClick={() => handleRemoveLine(index)} className="text-sm font-bold transition-colors duration-200 cursor-pointer hover:underline" style={{ color: 'var(--color-error)' }}>حذف</button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {editingLine && (
                <div className="mt-4 rounded-lg border p-5 space-y-4" style={{ backgroundColor: 'var(--color-surfaceContainerLow)', borderColor: 'var(--color-outlineVariant)' }}>
                  <h4 className="text-sm font-bold" style={{ color: 'var(--color-onSurface)' }}>سطر جديد</h4>
                  <div className="grid grid-cols-4 gap-4">
                    <div className="col-span-2">
                      <label className={labelClass} style={{ color: 'var(--color-onSurface)' }}>الحساب</label>
                      <select value={editingLine.accountId || ''} onChange={(e) => {
                        const sel = accounts.find((a) => a.id === Number(e.target.value));
                        setEditingLine({ ...editingLine, accountId: Number(e.target.value), accountCode: sel?.code ?? '', accountName: sel?.name ?? '' });
                      }} className={inputClass} style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }}>
                        <option value="">اختر الحساب...</option>
                        {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} - {a.name}</option>)}
                      </select>
                    </div>
                    <div>
                      <label className={labelClass} style={{ color: 'var(--color-onSurface)' }}>مدين</label>
                      <input type="number" value={editingLine.debit || ''} onChange={(e) => setEditingLine({ ...editingLine, debit: Number(e.target.value), credit: 0 })} className={inputClass}
                        style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
                    </div>
                    <div>
                      <label className={labelClass} style={{ color: 'var(--color-onSurface)' }}>دائن</label>
                      <input type="number" value={editingLine.credit || ''} onChange={(e) => setEditingLine({ ...editingLine, credit: Number(e.target.value), debit: 0 })} className={inputClass}
                        style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
                    </div>
                  </div>
                  <div>
                    <label className={labelClass} style={{ color: 'var(--color-onSurface)' }}>الوصف</label>
                    <input type="text" value={editingLine.description || ''} onChange={(e) => setEditingLine({ ...editingLine, description: e.target.value })} placeholder="وصف السطر..." className={inputClass}
                      style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
                  </div>
                  <div>
                    <label className={labelClass} style={{ color: 'var(--color-onSurface)' }}>الأبعاد التحليلية</label>
                    <DimensionPickers
                      value={{
                        fundId: editingLine.fundId,
                        projectId: editingLine.projectId,
                        budgetItemId: editingLine.budgetItemId,
                        encumbranceId: editingLine.encumbranceId,
                        paymentOrderId: editingLine.paymentOrderId,
                      }}
                      onChange={(dims: DimensionValues) => setEditingLine({ ...editingLine, ...dims })}
                    />
                  </div>
                  <div className="flex gap-2 pt-2">
                    <button type="button" onClick={handleSaveLine} className="px-4 py-2 text-sm font-bold rounded-lg transition-all duration-200 cursor-pointer hover:shadow-sm"
                      style={{ backgroundColor: 'var(--color-success)', color: 'var(--color-surface)' }}>حفظ السطر</button>
                    <button type="button" onClick={() => setEditingLine(null)} className="px-4 py-2 text-sm font-bold rounded-lg transition-all duration-200 cursor-pointer hover:shadow-sm"
                      style={{ backgroundColor: 'var(--color-surfaceContainer)', color: 'var(--color-onSurface)' }}>إلغاء</button>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* التوازن */}
          <div className={cardClass} style={{ backgroundColor: 'var(--color-surface)', ...cardBorder }}>
            <div className="p-6">
              <BalanceIndicator totalDebit={totalDebit} totalCredit={totalCredit} />
            </div>
          </div>
        </div>
      </form>

      <div className="mt-8 flex items-center gap-3">
        <button type="submit" form="journal-entry-form" disabled={!canSave || isSaving}
          className="px-6 py-2.5 rounded-lg text-sm font-bold transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer hover:shadow-md"
          style={{ backgroundColor: 'var(--color-primary)', color: 'var(--color-on-primary)' }}>
          {isSaving ? (
            <span className="flex items-center gap-2">
              <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
              جاري الحفظ...
            </span>
          ) : 'حفظ القيد'}
        </button>
        <button onClick={() => navigate('/accounting/journal-entries')} disabled={isSaving}
          className="px-6 py-2.5 rounded-lg text-sm font-bold transition-all duration-200 cursor-pointer hover:shadow-sm"
          style={{ backgroundColor: 'var(--color-surfaceContainer)', color: 'var(--color-onSurface)' }}>إلغاء</button>
      </div>
    </div>
  );
}
