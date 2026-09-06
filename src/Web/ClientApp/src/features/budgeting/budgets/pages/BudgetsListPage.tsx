import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useBudgetsList } from '../../hooks/useBudgets';
import { budgetStatusLabels, BudgetStatus, type BudgetFilters } from '../../shared/types';
import { Button, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus } from 'lucide-react';

const statusVariants: Record<string, 'default' | 'success' | 'warning' | 'danger' | 'info'> = {
  [BudgetStatus.Draft]: 'default',
  [BudgetStatus.Submitted]: 'warning',
  [BudgetStatus.Approved]: 'success',
  [BudgetStatus.Active]: 'info',
  [BudgetStatus.Suspended]: 'warning',
  [BudgetStatus.Closed]: 'default',
  [BudgetStatus.Cancelled]: 'danger',
};

const columns: DataGridColumn<any>[] = [
  { header: 'رقم الموازنة', cell: (row) => <span className="font-mono">{row.budgetNumber}</span> },
  { header: 'اسم الموازنة', cell: (row) => row.budgetName },
  { header: 'الصندوق', cell: (row) => row.fundName },
  { header: 'المبلغ', cell: (row) => <span className="font-mono">{row.totalAmount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span> },
  { header: 'الحالة', cell: (row) => <Badge variant={statusVariants[row.status] ?? 'default'}>{budgetStatusLabels[row.status] ?? String(row.status)}</Badge> },
];

export default function BudgetsListPage() {
  const navigate = useNavigate();
  const [filters] = useState<BudgetFilters>({});
  const { data: budgets, isLoading } = useBudgetsList(filters);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">الموازنات</h1>
        <Button variant="primary" size="sm" onClick={() => navigate('/budgeting/budgets/create')}>
          <Plus size={16} className="ms-1" />
          موازنة جديدة
        </Button>
      </div>

      <DataGrid
        columns={columns}
        data={budgets ?? []}
        loading={isLoading}
        emptyMessage="لا توجد موازنات بعد"
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/budgeting/budgets/${row.id}`)}
      />
    </div>
  );
}
