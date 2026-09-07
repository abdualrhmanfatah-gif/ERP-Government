import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateBudget } from '../../hooks/useBudgets';
import { useBudgetTypesList } from '../../hooks/useBudgetTypes';
import { useFundsList } from '../../hooks/useFunds';
import { useFiscalYearsList } from '../../../financial-settings/hooks/useFiscalYears';
import { Page, Button, Input, Select, Textarea } from '@/components/ui';
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
    <Page
      title="موازنة جديدة"
      maxWidth="sm"
      actions={
        <Button variant="ghost" size="icon" onClick={() => navigate('/budgeting/budgets')} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
      }
    >
      <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Input
            label="اسم الموازنة *"
            type="text"
            value={form.budgetName}
            onChange={(e) => updateField('budgetName', e.target.value)}
            required
          />
          {errors.budgetName && <p className="text-xs text-[var(--color-error)] mt-1">{errors.budgetName}</p>}

          <Select
            label="نوع الموازنة *"
            value={String(form.budgetTypeId)}
            onChange={(e) => updateField('budgetTypeId', Number(e.target.value))}
            options={[
              { value: '0', label: 'اختر النوع...' },
              ...budgetTypes.map((bt) => ({ value: String(bt.id), label: bt.name })),
            ]}
          />
          {errors.budgetTypeId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.budgetTypeId}</p>}

          <Select
            label="السنة المالية *"
            value={String(form.fiscalYearId)}
            onChange={(e) => updateField('fiscalYearId', Number(e.target.value))}
            options={[
              { value: '0', label: 'اختر السنة المالية...' },
              ...fiscalYears.map((fy) => ({ value: String(fy.id), label: fy.name })),
            ]}
          />
          {errors.fiscalYearId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.fiscalYearId}</p>}

          <Select
            label="الصندوق *"
            value={String(form.fundId)}
            onChange={(e) => updateField('fundId', Number(e.target.value))}
            options={[
              { value: '0', label: 'اختر الصندوق...' },
              ...funds.map((f) => ({ value: String(f.id), label: f.fundName })),
            ]}
          />
          {errors.fundId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.fundId}</p>}

          <Input
            label="المبلغ الإجمالي *"
            type="number"
            value={form.totalAmount || ''}
            onChange={(e) => updateField('totalAmount', Number(e.target.value))}
            min="0"
            step="0.01"
            required
          />
          {errors.totalAmount && <p className="text-xs text-[var(--color-error)] mt-1">{errors.totalAmount}</p>}

          <Input
            label="تاريخ البداية *"
            type="date"
            value={form.effectiveFrom}
            onChange={(e) => updateField('effectiveFrom', e.target.value)}
            required
          />
          {errors.effectiveFrom && <p className="text-xs text-[var(--color-error)] mt-1">{errors.effectiveFrom}</p>}

          <Input
            label="تاريخ النهاية"
            type="date"
            value={form.effectiveTo}
            onChange={(e) => updateField('effectiveTo', e.target.value)}
          />

          <div className="md:col-span-2">
            <Textarea
              label="الوصف"
              value={form.description}
              onChange={(e) => updateField('description', e.target.value)}
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
    </Page>
  );
}
