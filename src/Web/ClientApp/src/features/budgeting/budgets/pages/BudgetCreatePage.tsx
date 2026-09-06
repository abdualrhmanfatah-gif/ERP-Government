import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateBudget } from '../../hooks/useBudgets';
import { useBudgetTypesList } from '../../hooks/useBudgetTypes';
import { useFundsList } from '../../hooks/useFunds';
import { useFiscalYearsList } from '../../../financial-settings/hooks/useFiscalYears';
import { Button } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export default function BudgetCreatePage() {
  const navigate = useNavigate();
  const createBudget = useCreateBudget();
  const { data: budgetTypes = [] } = useBudgetTypesList();
  const { data: fiscalYears = [] } = useFiscalYearsList(true);
  const { data: funds = [] } = useFundsList();

  const [form, setForm] = useState({
    budgetName: '',
    budgetTypeId: 0,
    fiscalYearId: 0,
    fundId: 0,
    effectiveFrom: '',
    effectiveTo: '',
    description: '',
    totalAmount: 0,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.budgetName.trim()) e.budgetName = 'اسم الموازنة مطلوب';
    if (!form.budgetTypeId) e.budgetTypeId = 'نوع الموازنة مطلوب';
    if (!form.fiscalYearId) e.fiscalYearId = 'السنة المالية مطلوبة';
    if (!form.fundId) e.fundId = 'الصندوق مطلوب';
    if (!form.effectiveFrom) e.effectiveFrom = 'تاريخ البداية مطلوب';
    if (form.totalAmount <= 0) e.totalAmount = 'المبلغ يجب أن يكون أكبر من صفر';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    try {
      const result = await createBudget.mutateAsync({
        budgetName: form.budgetName,
        budgetTypeId: form.budgetTypeId,
        fiscalYearId: form.fiscalYearId,
        fundId: form.fundId,
        effectiveFrom: form.effectiveFrom,
        effectiveTo: form.effectiveTo || undefined,
        description: form.description || undefined,
        totalAmount: form.totalAmount,
      });
      navigate(`/budgeting/budgets/${result}`);
    } catch (err: unknown) {
      const problem = err as { status?: number; detail?: string };
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
        <Button variant="ghost" size="icon" onClick={() => navigate('/budgeting/budgets')} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">موازنة جديدة</h1>
      </div>

      <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">اسم الموازنة *</label>
            <input
              type="text"
              value={form.budgetName}
              onChange={(e) => updateField('budgetName', e.target.value)}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            />
            {errors.budgetName && <p className="text-xs text-[var(--color-error)] mt-1">{errors.budgetName}</p>}
          </div>

          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">نوع الموازنة *</label>
            <select
              value={form.budgetTypeId}
              onChange={(e) => updateField('budgetTypeId', Number(e.target.value))}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            >
              <option value={0}>اختر النوع...</option>
              {budgetTypes.map((bt) => (
                <option key={bt.id} value={bt.id}>{bt.name}</option>
              ))}
            </select>
            {errors.budgetTypeId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.budgetTypeId}</p>}
          </div>

          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">السنة المالية *</label>
            <select
              value={form.fiscalYearId}
              onChange={(e) => updateField('fiscalYearId', Number(e.target.value))}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            >
              <option value={0}>اختر السنة المالية...</option>
              {fiscalYears.map((fy) => (
                <option key={fy.id} value={fy.id}>{fy.name}</option>
              ))}
            </select>
            {errors.fiscalYearId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.fiscalYearId}</p>}
          </div>

          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الصندوق *</label>
            <select
              value={form.fundId}
              onChange={(e) => updateField('fundId', Number(e.target.value))}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            >
              <option value={0}>اختر الصندوق...</option>
              {funds.map((f) => (
                <option key={f.id} value={f.id}>{f.fundName}</option>
              ))}
            </select>
            {errors.fundId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.fundId}</p>}
          </div>

          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم المبلغ الإجمالي *</label>
            <input
              type="number"
              value={form.totalAmount || ''}
              onChange={(e) => updateField('totalAmount', Number(e.target.value))}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
              min="0"
              step="0.01"
            />
            {errors.totalAmount && <p className="text-xs text-[var(--color-error)] mt-1">{errors.totalAmount}</p>}
          </div>

          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">تاريخ البداية *</label>
            <input
              type="date"
              value={form.effectiveFrom}
              onChange={(e) => updateField('effectiveFrom', e.target.value)}
              className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
            />
            {errors.effectiveFrom && <p className="text-xs text-[var(--color-error)] mt-1">{errors.effectiveFrom}</p>}
          </div>

          <div>
            <label className="block text-xs text-[var(--color-on-surface-variant)] mb-1">تاريخ النهاية</label>
            <input
              type="date"
              value={form.effectiveTo}
              onChange={(e) => updateField('effectiveTo', e.target.value)}
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

        {errors.submit && <p className="text-sm text-[var(--color-error)] mt-4">{errors.submit}</p>}

        <div className="flex justify-end gap-2 mt-6">
          <Button variant="ghost" type="button" onClick={() => navigate('/budgeting/budgets')}>
            إلغاء
          </Button>
          <Button variant="primary" type="submit" disabled={createBudget.isPending}>
            {createBudget.isPending ? 'جارٍ الحفظ...' : 'حفظ'}
          </Button>
        </div>
      </form>
    </div>
  );
}
