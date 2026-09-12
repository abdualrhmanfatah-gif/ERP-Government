import { useState } from 'react';
import { Button, Input, Select, Textarea } from '@/components/ui';
import type { ComboboxOption } from '@/components/ui/Combobox';

interface BudgetFormProps {
  onSubmit: (data: { budgetName: string; budgetTypeId: number; fiscalYearId: number; fundId: number; description?: string }) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  initialData?: { budgetName: string; budgetTypeId: number; fiscalYearId: number; fundId: number; description?: string };
  onEdit?: () => void;
  budgetTypeOptions: ComboboxOption[];
  fiscalYearOptions: ComboboxOption[];
  fundOptions: ComboboxOption[];
}

export function BudgetForm({
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  initialData,
  onEdit,
  budgetTypeOptions,
  fiscalYearOptions,
  fundOptions,
}: BudgetFormProps) {
  const [form, setForm] = useState({
    budgetName: initialData?.budgetName ?? '',
    budgetTypeId: initialData?.budgetTypeId ?? 0,
    fiscalYearId: initialData?.fiscalYearId ?? 0,
    fundId: initialData?.fundId ?? 0,
    description: initialData?.description ?? '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.budgetName.trim()) e.budgetName = 'اسم الموازنة مطلوب';
    if (!form.budgetTypeId) e.budgetTypeId = 'نوع الموازنة مطلوب';
    if (!form.fiscalYearId) e.fiscalYearId = 'السنة المالية مطلوبة';
    if (!form.fundId) e.fundId = 'الصندوق مطلوب';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    onSubmit(form);
  }

  function updateField(field: string, value: string | number) {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  }

  if (readOnly && initialData) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">بيانات الموازنة</h3>
          {onEdit && <Button variant="outline" size="sm" onClick={onEdit}>تعديل</Button>}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div><span className="text-[var(--color-on-surface-variant)]">اسم الموازنة:</span> <span>{initialData.budgetName}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">نوع الموازنة:</span> <span>{budgetTypeOptions.find((o) => o.value === String(initialData.budgetTypeId))?.label ?? `#${initialData.budgetTypeId}`}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">السنة المالية:</span> <span>{fiscalYearOptions.find((o) => o.value === String(initialData.fiscalYearId))?.label ?? `#${initialData.fiscalYearId}`}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">الصندوق:</span> <span>{fundOptions.find((o) => o.value === String(initialData.fundId))?.label ?? `#${initialData.fundId}`}</span></div>
          {initialData.description && <div className="md:col-span-2"><span className="text-[var(--color-on-surface-variant)]">الوصف:</span> <span>{initialData.description}</span></div>}
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="موازنة">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <Input label="اسم الموازنة *" value={form.budgetName} onChange={(e) => updateField('budgetName', e.target.value)} required />
          {errors.budgetName && <p className="text-xs text-[var(--color-error)] mt-1">{errors.budgetName}</p>}
        </div>
        <div>
          <Select label="نوع الموازنة *" value={String(form.budgetTypeId)} onChange={(e) => updateField('budgetTypeId', Number(e.target.value))} options={[{ value: '0', label: 'اختر النوع...' }, ...budgetTypeOptions]} />
          {errors.budgetTypeId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.budgetTypeId}</p>}
        </div>
        <div>
          <Select label="السنة المالية *" value={String(form.fiscalYearId)} onChange={(e) => updateField('fiscalYearId', Number(e.target.value))} options={[{ value: '0', label: 'اختر السنة المالية...' }, ...fiscalYearOptions]} />
          {errors.fiscalYearId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.fiscalYearId}</p>}
        </div>
        <div>
          <Select label="الصندوق *" value={String(form.fundId)} onChange={(e) => updateField('fundId', Number(e.target.value))} options={[{ value: '0', label: 'اختر الصندوق...' }, ...fundOptions]} />
          {errors.fundId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.fundId}</p>}
        </div>
        <div className="md:col-span-2">
          <Textarea label="الوصف" value={form.description} onChange={(e) => updateField('description', e.target.value)} rows={3} />
        </div>
      </div>
      {errors.submit && <p className="text-sm text-[var(--color-error)] mt-4">{errors.submit}</p>}
      <div className="flex justify-end gap-2 mt-6">
        <Button variant="ghost" type="button" onClick={onCancel}>إلغاء</Button>
        <Button variant="primary" type="submit" disabled={isPending} loading={isPending}>حفظ</Button>
      </div>
    </form>
  );
}
