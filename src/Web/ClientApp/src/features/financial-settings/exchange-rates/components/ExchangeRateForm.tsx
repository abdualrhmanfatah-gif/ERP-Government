import { useState } from 'react';
import { Button, Input, Select } from '@/components/ui';
import { exchangeRateTypeLabels } from '../../shared/types';
import type { ComboboxOption } from '@/components/ui/Combobox';

interface ExchangeRateFormProps {
  onSubmit: (data: { baseCurrencyId: number; currencyId: number; rateDate: string; rateType: number; rate: number }) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  initialData?: { baseCurrencyId: number; currencyId: number; rateDate: string; rateType: number; rate: number };
  onEdit?: () => void;
  currencyOptions: ComboboxOption[];
}

export function ExchangeRateForm({
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  initialData,
  onEdit,
  currencyOptions,
}: ExchangeRateFormProps) {
  const [form, setForm] = useState({
    baseCurrencyId: initialData?.baseCurrencyId ?? 0,
    currencyId: initialData?.currencyId ?? 0,
    rateDate: initialData?.rateDate ?? new Date().toISOString().split('T')[0],
    rateType: initialData?.rateType ?? 0,
    rate: initialData?.rate ?? 0,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.baseCurrencyId) e.baseCurrencyId = 'العملة الأساسية مطلوبة';
    if (!form.currencyId) e.currencyId = 'العملة مطلوبة';
    if (form.baseCurrencyId === form.currencyId) e.currencyId = 'يجب أن تكون العملات مختلفة';
    if (!form.rateDate) e.rateDate = 'التاريخ مطلوب';
    if (!form.rate || form.rate <= 0) e.rate = 'السعر يجب أن يكون أكبر من صفر';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    onSubmit(form);
  }

  if (readOnly && initialData) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">بيانات سعر الصرف</h3>
          {onEdit && <Button variant="outline" size="sm" onClick={onEdit}>تعديل</Button>}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div><span className="text-[var(--color-on-surface-variant)]">العملة الأساسية:</span> <span>{currencyOptions.find((o) => o.value === String(initialData.baseCurrencyId))?.label ?? `#${initialData.baseCurrencyId}`}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">العملة:</span> <span>{currencyOptions.find((o) => o.value === String(initialData.currencyId))?.label ?? `#${initialData.currencyId}`}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">التاريخ:</span> <span>{new Date(initialData.rateDate).toLocaleDateString('ar-YE')}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">النوع:</span> <span>{exchangeRateTypeLabels[initialData.rateType as keyof typeof exchangeRateTypeLabels] ?? initialData.rateType}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">السعر:</span> <span>{initialData.rate}</span></div>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="سعر صرف">
      <div className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Select label="العملة الأساسية *" value={String(form.baseCurrencyId)} onChange={(e) => setForm((p) => ({ ...p, baseCurrencyId: Number(e.target.value) }))} options={[{ value: '0', label: 'اختر...' }, ...currencyOptions]} />
            {errors.baseCurrencyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.baseCurrencyId}</p>}
          </div>
          <div>
            <Select label="العملة *" value={String(form.currencyId)} onChange={(e) => setForm((p) => ({ ...p, currencyId: Number(e.target.value) }))} options={[{ value: '0', label: 'اختر...' }, ...currencyOptions]} />
            {errors.currencyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.currencyId}</p>}
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Input label="تاريخ السعر *" type="date" value={form.rateDate} onChange={(e) => setForm((p) => ({ ...p, rateDate: e.target.value }))} required />
            {errors.rateDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.rateDate}</p>}
          </div>
          <Select label="نوع السعر *" value={String(form.rateType)} onChange={(e) => setForm((p) => ({ ...p, rateType: Number(e.target.value) }))} options={Object.entries(exchangeRateTypeLabels).map(([value, label]) => ({ value, label }))} />
        </div>
        <div>
          <Input label="السعر *" type="number" step="0.000001" min="0" value={form.rate || ''} onChange={(e) => setForm((p) => ({ ...p, rate: Number(e.target.value) }))} required />
          {errors.rate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.rate}</p>}
        </div>
      </div>
      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="ghost" onClick={onCancel}>إلغاء</Button>
        <Button type="submit" disabled={isPending}>{isPending ? 'جارٍ الحفظ...' : 'حفظ'}</Button>
      </div>
    </form>
  );
}
