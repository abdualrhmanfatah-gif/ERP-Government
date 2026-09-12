import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Input, Select, Textarea, DatePicker } from '@/components/ui';
import { encumbranceTypeLabels, EncumbranceType } from '../../shared/types';
import { useEncumbranceDetail } from '../../hooks/useEncumbrances';

interface EncumbranceFormProps {
  onSubmit: (data: Record<string, unknown>) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  initialData?: Record<string, unknown>;
  onEdit?: () => void;
}

export function EncumbranceForm({
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  initialData,
  onEdit,
}: EncumbranceFormProps) {
  const [showReversal, setShowReversal] = useState(false);

  const [form, setForm] = useState({
    encumbranceType: (initialData?.encumbranceType as number) ?? EncumbranceType.Commitment,
    budgetItemId: (initialData?.budgetItemId as number) ?? 0,
    amount: (initialData?.amount as number) ?? 0,
    encumbranceDate: (initialData?.encumbranceDate as string) ?? new Date().toISOString().split('T')[0],
    vendorId: (initialData?.vendorId as number) ?? 0,
    documentType: (initialData?.documentType as string) ?? '',
    documentId: (initialData?.documentId as number) ?? 0,
    description: (initialData?.description as string) ?? '',
    reversalOfId: 0,
    reversalReason: '',
  });

  const { data: reversalTarget } = useEncumbranceDetail(form.reversalOfId);
  const [errors, setErrors] = useState<Record<string, string>>({});

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.budgetItemId) e.budgetItemId = 'رقم البند مطلوب';
    if (form.amount <= 0) e.amount = 'المبلغ يجب أن يكون أكبر من صفر';
    if (!form.documentType.trim()) e.documentType = 'نوع المستند مطلوب';
    if (showReversal && !form.reversalReason.trim()) e.reversalReason = 'سبب العكس مطلوب';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    if (showReversal) {
      if (!reversalTarget) { setErrors({ reversalOfId: 'الالتزام غير موجود' }); return; }
      onSubmit({ type: 'reversal', id: form.reversalOfId, rowVersion: reversalTarget.rowVersion, reason: form.reversalReason });
    } else {
      onSubmit({
        encumbranceType: form.encumbranceType,
        budgetItemId: form.budgetItemId,
        amount: form.amount,
        encumbranceDate: form.encumbranceDate,
        vendorId: form.vendorId || undefined,
        documentType: form.documentType,
        documentId: form.documentId || undefined,
        description: form.description || undefined,
      });
    }
  }

  function updateField(field: string, value: string | number) {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  }

  if (readOnly && initialData) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">بيانات الالتزام</h3>
          {onEdit && <Button variant="outline" size="sm" onClick={onEdit}>تعديل</Button>}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div><span className="text-[var(--color-on-surface-variant)]">النوع:</span> <span>{encumbranceTypeLabels[initialData.encumbranceType as keyof typeof encumbranceTypeLabels] ?? initialData.encumbranceType}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">رقم البند:</span> <span>{String(initialData.budgetItemId)}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">المبلغ:</span> <span>{Number(initialData.amount).toLocaleString('ar-YE')}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">التاريخ:</span> <span>{String(initialData.encumbranceDate)}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">نوع المستند:</span> <span>{String(initialData.documentType)}</span></div>
          {initialData.vendorId && <div><span className="text-[var(--color-on-surface-variant)]">رقم المورد:</span> <span>{String(initialData.vendorId)}</span></div>}
          {initialData.description && <div className="md:col-span-2"><span className="text-[var(--color-on-surface-variant)]">الوصف:</span> <span>{String(initialData.description)}</span></div>}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {!showReversal && (
        <Button variant="ghost" size="sm" onClick={() => setShowReversal(true)}>عكس التزام موجود</Button>
      )}
      <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="التزام">
        {showReversal ? (
          <div className="space-y-4">
            <Input label="رقم الالتزام المراد عكسه *" type="number" value={form.reversalOfId || ''} onChange={(e) => updateField('reversalOfId', Number(e.target.value))} />
            <Textarea label="سبب العكس *" value={form.reversalReason} onChange={(e) => updateField('reversalReason', e.target.value)} rows={3} error={errors.reversalReason} />
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <Select label="نوع الالتزام *" value={form.encumbranceType} onChange={(e) => updateField('encumbranceType', Number(e.target.value))} options={Object.entries(encumbranceTypeLabels).map(([val, label]) => ({ value: val, label }))} />
            <div><Input label="رقم البند *" type="number" value={form.budgetItemId || ''} onChange={(e) => updateField('budgetItemId', Number(e.target.value))} error={errors.budgetItemId} /></div>
            <div><Input label="المبلغ *" type="number" value={form.amount || ''} onChange={(e) => updateField('amount', Number(e.target.value))} min="0" step="0.01" error={errors.amount} /></div>
            <DatePicker label="تاريخ الالتزام *" value={form.encumbranceDate} onChange={(v) => updateField('encumbranceDate', v)} />
            <div><Input label="نوع المستند *" type="text" value={form.documentType} onChange={(e) => updateField('documentType', e.target.value)} error={errors.documentType} /></div>
            <Input label="رقم المورد" type="number" value={form.vendorId || ''} onChange={(e) => updateField('vendorId', Number(e.target.value))} />
            <div className="md:col-span-2"><Textarea label="الوصف" value={form.description} onChange={(e) => updateField('description', e.target.value)} rows={3} /></div>
          </div>
        )}
        {errors.submit && <p className="text-sm text-[var(--color-error)] mt-4">{errors.submit}</p>}
        <div className="flex justify-end gap-2 mt-6">
          {showReversal && <Button variant="ghost" type="button" onClick={() => setShowReversal(false)}>إنشاء التزام جديد</Button>}
          <Button variant="ghost" type="button" onClick={onCancel}>إلغاء</Button>
          <Button variant="primary" type="submit" loading={isPending}>حفظ</Button>
        </div>
      </form>
    </div>
  );
}
