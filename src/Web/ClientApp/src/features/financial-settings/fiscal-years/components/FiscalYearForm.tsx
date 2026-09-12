import { useState } from 'react';
import { Button, Input } from '@/components/ui';

interface FiscalYearFormProps {
  onSubmit: (data: { name: string; startDate: string; endDate: string }) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  initialData?: { name: string; startDate: string; endDate: string };
  onEdit?: () => void;
}

export function FiscalYearForm({
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  initialData,
  onEdit,
}: FiscalYearFormProps) {
  const [form, setForm] = useState({
    name: initialData?.name ?? '',
    startDate: initialData?.startDate ?? '',
    endDate: initialData?.endDate ?? '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.name.trim()) e.name = 'الاسم مطلوب';
    if (!form.startDate) e.startDate = 'تاريخ البداية مطلوب';
    if (!form.endDate) e.endDate = 'تاريخ النهاية مطلوب';
    if (form.startDate && form.endDate && new Date(form.startDate) >= new Date(form.endDate)) {
      e.endDate = 'تاريخ النهاية يجب أن يكون بعد تاريخ البداية';
    }
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
          <h3 className="text-lg font-semibold">بيانات السنة المالية</h3>
          {onEdit && <Button variant="outline" size="sm" onClick={onEdit}>تعديل</Button>}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div><span className="text-[var(--color-on-surface-variant)]">الاسم:</span> <span>{initialData.name}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">تاريخ البداية:</span> <span>{new Date(initialData.startDate).toLocaleDateString('ar-YE')}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">تاريخ النهاية:</span> <span>{new Date(initialData.endDate).toLocaleDateString('ar-YE')}</span></div>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="سنة مالية">
      <div className="space-y-4">
        <div>
          <Input label="اسم السنة المالية *" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} required />
          {errors.name && <p className="text-xs text-[var(--color-error)] mt-1">{errors.name}</p>}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Input label="تاريخ البداية *" type="date" value={form.startDate} onChange={(e) => setForm((p) => ({ ...p, startDate: e.target.value }))} required />
            {errors.startDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.startDate}</p>}
          </div>
          <div>
            <Input label="تاريخ النهاية *" type="date" value={form.endDate} onChange={(e) => setForm((p) => ({ ...p, endDate: e.target.value }))} required />
            {errors.endDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.endDate}</p>}
          </div>
        </div>
      </div>
      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="ghost" onClick={onCancel}>إلغاء</Button>
        <Button type="submit" disabled={isPending} loading={isPending}>حفظ</Button>
      </div>
    </form>
  );
}
