import { useState, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useCreateAppropriation, useCreateTransfer } from '../../hooks/useAppropriations';
import { useBudgetItemsTree } from '../../hooks/useBudgetItems';
import { AvailabilityIndicator } from '../../components/AvailabilityIndicator';
import { appropriationTypeLabels, AppropriationType } from '../../shared/types';
import { Button } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export default function AppropriationCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const budgetItemIdParam = searchParams.get('budgetItemId');
  const budgetIdParam = searchParams.get('budgetId');

  const createAppropriation = useCreateAppropriation();
  const createTransfer = useCreateTransfer();
  const { data: tree } = useBudgetItemsTree(budgetIdParam ? Number(budgetIdParam) : undefined);

  const [form, setForm] = useState({
    appropriationType: AppropriationType.Original,
    amount: 0,
    documentType: '',
    documentId: 0,
    budgetItemId: budgetItemIdParam ? Number(budgetItemIdParam) : 0,
    targetBudgetItemId: 0,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const isTransfer = form.appropriationType === AppropriationType.Transfer;
  const isPending = createAppropriation.isPending || createTransfer.isPending;

  const targetItems = useMemo(() => {
    if (!tree || !isTransfer) return [];
    const flat = tree.flatMap(function flatten(nodes: typeof tree): typeof tree {
      return nodes.flatMap((n) => [n, ...(n.children ? flatten(n.children) : [])]);
    });
    return flat.filter((item) => {
      if (item.id === form.budgetItemId) return false;
      if (!item.isActive) return false;
      return true;
    });
  }, [tree, isTransfer, form.budgetItemId]);

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.budgetItemId) e.budgetItemId = 'يجب اختيار بند الموازنة';
    if (form.amount <= 0) e.amount = 'المبلغ يجب أن يكون أكبر من صفر';
    if (!isTransfer && !form.documentType.trim()) e.documentType = 'نوع المستند مطلوب';
    if (isTransfer && !form.targetBudgetItemId) e.targetBudgetItemId = 'يرجى اختيار بند الهدف';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isTransfer) {
        await createTransfer.mutateAsync({
          budgetId: budgetIdParam ? Number(budgetIdParam) : 0,
          sourceBudgetItemId: form.budgetItemId,
          targetBudgetItemId: form.targetBudgetItemId,
          amount: form.amount,
        });
      } else {
        await createAppropriation.mutateAsync({
          budgetId: budgetIdParam ? Number(budgetIdParam) : 0,
          budgetItemId: form.budgetItemId,
          appropriationType: form.appropriationType,
          amount: form.amount,
          documentType: form.documentType,
          documentId: form.documentId,
        });
      }
      navigate('/budgeting/appropriations');
    } catch (err: unknown) {
      const problem = err as { detail?: string };
      setErrors({ submit: problem.detail ?? 'حدث خطأ أثناء الحفظ' });
    }
  }

  function updateField(field: string, value: string | number) {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">تخصيص جديد</h1>
      </div>

      {form.budgetItemId > 0 && (
        <AvailabilityIndicator budgetItemId={form.budgetItemId} />
      )}

      <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">نوع التخصيص *</label>
            <select
              value={form.appropriationType}
              onChange={(e) => updateField('appropriationType', e.target.value)}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            >
              {Object.entries(appropriationTypeLabels).map(([val, label]) => (
                <option key={val} value={val}>{label}</option>
              ))}
            </select>
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

          {!isTransfer && (
            <>
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
                <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم المستند</label>
                <input
                  type="number"
                  value={form.documentId || ''}
                  onChange={(e) => updateField('documentId', Number(e.target.value))}
                  className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
                />
              </div>
            </>
          )}

          {isTransfer && (
            <div className="md:col-span-2">
              <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">بند الهدف *</label>
              <select
                value={form.targetBudgetItemId}
                onChange={(e) => updateField('targetBudgetItemId', Number(e.target.value))}
                className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              >
                <option value={0}>اختر بند الهدف</option>
                {targetItems.map((item) => (
                  <option key={item.id} value={item.id}>{item.itemCode} — {item.itemName}</option>
                ))}
              </select>
              {errors.targetBudgetItemId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.targetBudgetItemId}</p>}
            </div>
          )}
        </div>

        {errors.submit && <p className="text-sm text-[var(--color-error)] mt-4">{errors.submit}</p>}

        <div className="flex justify-end gap-2 mt-6">
          <Button variant="ghost" type="button" onClick={() => navigate(-1)}>إلغاء</Button>
          <Button variant="primary" type="submit" disabled={isPending}>
            {isPending ? 'جارٍ الحفظ...' : 'حفظ'}
          </Button>
        </div>
      </form>
    </div>
  );
}
