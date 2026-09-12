import { useState } from 'react';
import { Button, Input, Select, DatePicker } from '@/components/ui';
import { FREQUENCY_LABELS } from '../shared/types';
import type { CreateRecurringEntryCommand } from '../shared/types';

interface RecurringEntryFormProps {
  initialData?: Partial<CreateRecurringEntryCommand>;
  onSubmit: (data: CreateRecurringEntryCommand) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  onEdit?: () => void;
}

export function RecurringEntryForm({
  initialData,
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  onEdit,
}: RecurringEntryFormProps) {
  const [form, setForm] = useState<CreateRecurringEntryCommand>({
    journalId: initialData?.journalId ?? 0,
    name: initialData?.name ?? '',
    frequency: initialData?.frequency ?? 'Monthly',
    startDate: initialData?.startDate ?? new Date(),
    endDate: initialData?.endDate,
    amount: initialData?.amount,
    templateId: initialData?.templateId,
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onSubmit(form);
  }

  if (readOnly && initialData) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">بيانات الجدول الدوري</h3>
          {onEdit && <Button variant="outline" size="sm" onClick={onEdit}>تعديل</Button>}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div><span className="text-[var(--color-on-surface-variant)]">الاسم:</span> <span>{initialData.name}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">رقم الدفتر:</span> <span>{initialData.journalId}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">الدورية:</span> <span>{FREQUENCY_LABELS[initialData.frequency!] ?? initialData.frequency}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">تاريخ البداية:</span> <span>{initialData.startDate ? new Date(initialData.startDate).toLocaleDateString('ar-YE') : '—'}</span></div>
          {initialData.endDate && <div><span className="text-[var(--color-on-surface-variant)]">تاريخ النهاية:</span> <span>{new Date(initialData.endDate).toLocaleDateString('ar-YE')}</span></div>}
          {initialData.amount != null && <div><span className="text-[var(--color-on-surface-variant)]">المبلغ:</span> <span>{initialData.amount}</span></div>}
          {initialData.templateId && <div><span className="text-[var(--color-on-surface-variant)]">رقم القالب:</span> <span>{initialData.templateId}</span></div>}
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="قيد دوري">
      <div className="space-y-4">
        <Input label="اسم الجدول *" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} required />
        <Input label="رقم الدفتر *" type="number" value={form.journalId || ''} onChange={(e) => setForm((p) => ({ ...p, journalId: Number(e.target.value) }))} required />
        <Select label="الدورية *" value={form.frequency} onChange={(e) => setForm((p) => ({ ...p, frequency: e.target.value as any }))} options={Object.entries(FREQUENCY_LABELS).map(([key, label]) => ({ value: key, label }))} />
        <DatePicker label="تاريخ البداية *" value={form.startDate instanceof Date ? form.startDate.toISOString().split('T')[0] : ''} onChange={(v) => setForm((p) => ({ ...p, startDate: new Date(v) }))} required />
        <DatePicker label="تاريخ النهاية" value={form.endDate instanceof Date ? form.endDate.toISOString().split('T')[0] : ''} onChange={(v) => setForm((p) => ({ ...p, endDate: v ? new Date(v) : undefined }))} />
        <Input label="المبلغ" type="number" value={form.amount || ''} onChange={(e) => setForm((p) => ({ ...p, amount: e.target.value ? Number(e.target.value) : undefined }))} />
        <Input label="رقم القالب" type="number" value={form.templateId || ''} onChange={(e) => setForm((p) => ({ ...p, templateId: e.target.value ? Number(e.target.value) : undefined }))} />
      </div>
      <div className="flex gap-2 justify-end pt-4">
        <Button variant="outline" type="button" onClick={onCancel}>إلغاء</Button>
        <Button variant="primary" type="submit" loading={isPending}>حفظ</Button>
      </div>
    </form>
  );
}
