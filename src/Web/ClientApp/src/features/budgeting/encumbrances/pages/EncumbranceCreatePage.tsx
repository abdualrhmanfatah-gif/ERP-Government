import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useCreateEncumbrance, useReverseEncumbrance, useEncumbranceAvailability } from '../../hooks/useEncumbrances';
import { AvailabilityIndicator } from '@/components/BudgetingAvailabilityIndicator';
import { encumbranceTypeLabels, EncumbranceType } from '../../shared/types';
import { Button, Card } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export default function EncumbranceCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const appropriationIdParam = searchParams.get('appropriationId');

  const createEncumbrance = useCreateEncumbrance();
  const reverseEncumbrance = useReverseEncumbrance();
  const [showReversal, setShowReversal] = useState(false);

  const [form, setForm] = useState({
    encumbranceType: EncumbranceType.Commitment,
    appropriationId: appropriationIdParam ? Number(appropriationIdParam) : 0,
    amount: 0,
    encumbranceDate: new Date().toISOString().split('T')[0],
    vendorId: 0,
    documentType: '',
    documentId: 0,
    description: '',
    reversalOfId: 0,
    reversalReason: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [availabilityKey, setAvailabilityKey] = useState(0);
  const { data: encumbranceAvailability } = useEncumbranceAvailability(
    form.appropriationId > 0 ? form.appropriationId : undefined,
  );

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.appropriationId) e.appropriationId = 'رقم التخصيص مطلوب';
    if (form.amount <= 0) e.amount = 'المبلغ يجب أن يكون أكبر من صفر';
    if (!form.documentType.trim()) e.documentType = 'نوع المستند مطلوب';
    if (showReversal && !form.reversalReason.trim()) e.reversalReason = 'سبب العكس مطلوب';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (showReversal) {
        await reverseEncumbrance.mutateAsync({
          id: form.reversalOfId,
          rowVersion: '',
          reason: form.reversalReason,
        });
      } else {
        await createEncumbrance.mutateAsync({
          encumbranceType: form.encumbranceType,
          appropriationId: form.appropriationId,
          amount: form.amount,
          encumbranceDate: form.encumbranceDate,
          vendorId: form.vendorId || undefined,
          documentType: form.documentType,
          documentId: form.documentId || undefined,
          description: form.description || undefined,
        });
      }
      navigate('/budgeting/encumbrances');
    } catch (err: unknown) {
      const problem = err as { detail?: string };
      setErrors({ submit: problem.detail ?? 'حدث خطأ أثناء الحفظ' });
    }
  }

  function updateField(field: string, value: string | number) {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
    if (field === 'appropriationId') setAvailabilityKey((k) => k + 1);
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">
          {showReversal ? 'عكس التزام' : 'التزام جديد'}
        </h1>
      </div>

      {form.appropriationId > 0 && !showReversal && (
        <AvailabilityIndicator key={availabilityKey} appropriationId={form.appropriationId} />
      )}

      {encumbranceAvailability && (
        <Card padding="sm" className="bg-[var(--color-surface-container-lowest)]">
          <div className="flex items-center justify-between text-sm">
            <span className="text-[var(--color-on-surface-variant)]">المتوفر للالتزام</span>
            <span className="font-mono text-[var(--color-primary)]">
              {encumbranceAvailability.available.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}
            </span>
          </div>
          {encumbranceAvailability.warning && (
            <p className="text-xs text-[var(--color-warning)] mt-2">{encumbranceAvailability.warning}</p>
          )}
        </Card>
      )}

      {!showReversal && (
        <Button variant="ghost" size="sm" onClick={() => setShowReversal(true)}>
          عكس التزام موجود
        </Button>
      )}

      <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        {showReversal ? (
          <div className="space-y-4">
            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم الالتزام المراد عكسه *</label>
              <input
                type="number"
                value={form.reversalOfId || ''}
                onChange={(e) => updateField('reversalOfId', Number(e.target.value))}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">سبب العكس *</label>
              <textarea
                value={form.reversalReason}
                onChange={(e) => updateField('reversalReason', e.target.value)}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
                rows={3}
              />
              {errors.reversalReason && <p className="text-xs text-[var(--color-error)] mt-1">{errors.reversalReason}</p>}
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">نوع الالتزام *</label>
              <select
                value={form.encumbranceType}
                onChange={(e) => updateField('encumbranceType', Number(e.target.value))}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              >
                {Object.entries(encumbranceTypeLabels).map(([val, label]) => (
                  <option key={val} value={val}>{label}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم التخصيص *</label>
              <input
                type="number"
                value={form.appropriationId || ''}
                onChange={(e) => updateField('appropriationId', Number(e.target.value))}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              />
              {errors.appropriationId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.appropriationId}</p>}
            </div>

            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">المبلغ *</label>
              <input
                type="number"
                value={form.amount || ''}
                onChange={(e) => updateField('amount', Number(e.target.value))}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
                min="0"
                step="0.01"
              />
              {errors.amount && <p className="text-xs text-[var(--color-error)] mt-1">{errors.amount}</p>}
            </div>

            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">تاريخ الالتزام *</label>
              <input
                type="date"
                value={form.encumbranceDate}
                onChange={(e) => updateField('encumbranceDate', e.target.value)}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              />
            </div>

            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">نوع المستند *</label>
              <input
                type="text"
                value={form.documentType}
                onChange={(e) => updateField('documentType', e.target.value)}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              />
              {errors.documentType && <p className="text-xs text-[var(--color-error)] mt-1">{errors.documentType}</p>}
            </div>

            <div>
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم المورد</label>
              <input
                type="number"
                value={form.vendorId || ''}
                onChange={(e) => updateField('vendorId', Number(e.target.value))}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الوصف</label>
              <textarea
                value={form.description}
                onChange={(e) => updateField('description', e.target.value)}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
                rows={3}
              />
            </div>
          </div>
        )}

        {errors.submit && <p className="text-sm text-[var(--color-error)] mt-4">{errors.submit}</p>}

        <div className="flex justify-end gap-2 mt-6">
          {showReversal && (
            <Button variant="ghost" type="button" onClick={() => setShowReversal(false)}>
              إنشاء التزام جديد
            </Button>
          )}
          <Button variant="ghost" type="button" onClick={() => navigate(-1)}>إلغاء</Button>
          <Button variant="primary" type="submit" disabled={createEncumbrance.isPending || reverseEncumbrance.isPending}>
            {(createEncumbrance.isPending || reverseEncumbrance.isPending) ? 'جارٍ الحفظ...' : 'حفظ'}
          </Button>
        </div>
      </form>
    </div>
  );
}
