import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useBudgetsList } from '../../hooks/useBudgets';
import { budgetStatusLabels, BudgetStatus, type BudgetFilters } from '../../shared/types';
import { Button, Badge } from '@/components/ui';
import { Plus } from 'lucide-react';

const statusVariants: Record<number, 'default' | 'success' | 'warning' | 'danger' | 'info'> = {
  [BudgetStatus.Draft]: 'default',
  [BudgetStatus.Submitted]: 'warning',
  [BudgetStatus.Approved]: 'success',
  [BudgetStatus.Active]: 'info',
  [BudgetStatus.Suspended]: 'warning',
  [BudgetStatus.Closed]: 'default',
  [BudgetStatus.Cancelled]: 'danger',
};

export default function BudgetsListPage() {
  const navigate = useNavigate();
  const [filters] = useState<BudgetFilters>({});
  const { data: budgets, isLoading } = useBudgetsList(filters);

  if (isLoading) {
    return (
      <div className="p-6 space-y-4">
        <div className="h-8 w-48 rounded bg-[var(--color-surface-container)] animate-pulse" />
        <div className="h-64 rounded-lg bg-[var(--color-surface-container)] animate-pulse" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">الموازنات</h1>
        <Button variant="primary" size="sm" onClick={() => navigate('/budgeting/budgets/create')}>
          <Plus size={16} className="ms-1" />
          موازنة جديدة
        </Button>
      </div>

      {!budgets || budgets.length === 0 ? (
        <div className="rounded-lg border border-dashed border-[var(--color-outline)] p-12 text-center">
          <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد موازنات بعد</p>
          <Button variant="primary" size="sm" className="mt-3" onClick={() => navigate('/budgeting/budgets/create')}>
            إنشاء أول موازنة
          </Button>
        </div>
      ) : (
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container)]">
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">رقم الموازنة</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">اسم الموازنة</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">الصندوق</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">المبلغ</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">الحالة</th>
              </tr>
            </thead>
            <tbody>
              {budgets.map((budget) => (
                <tr
                  key={budget.id}
                  className="border-b border-[var(--color-outline-variant)] last:border-0 hover:bg-[var(--color-surface-variant)] cursor-pointer"
                  onClick={() => navigate(`/budgeting/budgets/${budget.id}`)}
                >
                  <td className="px-4 py-3 font-mono">{budget.budgetNumber}</td>
                  <td className="px-4 py-3">{budget.budgetName}</td>
                  <td className="px-4 py-3">{budget.fundName}</td>
                  <td className="px-4 py-3 font-mono">
                    {budget.totalAmount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={statusVariants[budget.status] ?? 'default'}>
                      {budgetStatusLabels[budget.status] ?? String(budget.status)}
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
