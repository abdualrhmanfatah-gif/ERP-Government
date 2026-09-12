import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { TrendingUp, Clock, Wallet, Plus, Activity } from 'lucide-react';
import { Page, Button, Loading, ErrorState, DataGrid, StatusBadge } from '@/components/ui';
import type { DataGridColumn } from '@/components/ui/DataGrid';
import { DashboardCard } from '../components/DashboardCard';
import { BudgetVsActualChart } from '../components/DashboardBudgetVsActualChart';
import { RevenueTrendChart } from '../components/DashboardRevenueTrendChart';
import { ExpenseBreakdownChart } from '../components/DashboardExpenseBreakdownChart';
import { PendingApprovalsList } from '../components/DashboardPendingApprovalsList';
import { useBudgetUtilization } from '../hooks/useBudgetUtilization';
import { usePendingApprovalsCount } from '../hooks/usePendingApprovalsCount';
import { useCashPosition } from '../hooks/useCashPosition';
import { useTotalTransactions } from '../hooks/useTotalTransactions';
import { useRecentTransactions } from '../hooks/useRecentTransactions';
import { statusMap } from '../shared/types';
import { formatDateCompact, formatCurrency } from '@/shared/utils/formatters';

interface Transaction {
  id: number;
  date: string;
  description: string;
  account: string;
  debit: number;
  credit: number;
  status: string;
}

const txColumns: DataGridColumn<Transaction>[] = [
  { header: 'التاريخ', cell: (row) => formatDateCompact(row.date) },
  { header: 'الوصف', cell: (row) => <span className="font-medium">{row.description}</span> },
  { header: 'الحساب', cell: (row) => row.account },
  { header: 'المدين', align: 'left', cell: (row) => row.debit > 0 ? <span className="text-[var(--color-error)]">{formatCurrency(row.debit)}</span> : <span className="text-[var(--color-on-surface-variant)]">—</span> },
  { header: 'الدائن', align: 'left', cell: (row) => row.credit > 0 ? <span className="text-[var(--color-success)]">{formatCurrency(row.credit)}</span> : <span className="text-[var(--color-on-surface-variant)]">—</span> },
  { header: 'الحالة', align: 'center', cell: (row) => { const s = statusMap[row.status]; return s ? <StatusBadge variant={s.variant} size="sm">{s.label}</StatusBadge> : row.status; } },
];

export function DashboardPage() {
  const navigate = useNavigate();
  const budget = useBudgetUtilization();
  const approvals = usePendingApprovalsCount();
  const cash = useCashPosition();
  const transactions = useTotalTransactions();
  const recentTx = useRecentTransactions();

  const budgetValue = useMemo(() => {
    if (budget.data?.utilizationPercent === null) return undefined;
    if (budget.data?.utilizationPercent !== undefined) {
      return `${budget.data.utilizationPercent.toFixed(1)}%`;
    }
    return undefined;
  }, [budget.data]);

  const budgetEmpty = budget.data?.utilizationPercent === null ? 'لا توجد ميزانيات' : undefined;
  const cashEmpty = cash.data && !cash.data.hasBankAccounts ? 'لا توجد حسابات بنكية' : undefined;

  return (
    <Page
      title="لوحة التحكم"
      actions={
        <Button variant="primary" icon={<Plus size={16} />} onClick={() => navigate('/accounting/journal-entries/new')}>
          إجراء سريع
        </Button>
      }
    >
      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        <DashboardCard
          title="نسبة تنفيذ الميزانية"
          onClick={() => navigate('/budget/budgets')}
        >
          {budget.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : budget.error ? (
            <ErrorState message={budget.error.message} onRetry={() => budget.refetch()} />
          ) : (
            <div className="flex flex-col h-full">
              <div className="flex items-center justify-between">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {budgetEmpty ? '—' : budgetValue}
                </span>
                <TrendingUp size={18} className={budgetEmpty ? 'text-[var(--color-on-surface-variant)]' : 'text-[var(--color-primary)]'} />
              </div>
              <p className="text-xs text-[var(--color-on-surface-variant)] mt-auto">
                {budgetEmpty ?? (budget.data ? `${formatCurrency(budget.data.totalSpent)} / ${formatCurrency(budget.data.totalAllocated)}` : '—')}
              </p>
            </div>
          )}
        </DashboardCard>

        <DashboardCard
          title="الموافقات المعلقة"
          onClick={() => navigate('/approval-rules/pending')}
        >
          {approvals.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : approvals.error ? (
            <ErrorState message={approvals.error.message} onRetry={() => approvals.refetch()} />
          ) : (
            <div className="flex flex-col h-full">
              <div className="flex items-center justify-between">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {approvals.data?.count ?? 0}
                </span>
                <Clock size={18} className={approvals.data?.count === 0 ? 'text-[var(--color-on-surface-variant)]' : 'text-[var(--color-primary)]'} />
              </div>
              <p className="text-xs text-[var(--color-on-surface-variant)] mt-auto">
                {approvals.data?.count === 0 ? 'لا توجد موافقات معلقة' : 'موافقة معلقة'}
              </p>
            </div>
          )}
        </DashboardCard>

        <DashboardCard
          title="الموقف النقدي"
          onClick={() => navigate('/banking/bank-accounts')}
        >
          {cash.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : cash.error ? (
            <ErrorState message={cash.error.message} onRetry={() => cash.refetch()} />
          ) : (
            <div className="flex flex-col h-full">
              <div className="flex items-center justify-between">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {cashEmpty ? '—' : (cash.data?.hasBankAccounts ? formatCurrency(cash.data.totalBalance) : '—')}
                </span>
                <Wallet size={18} className={cashEmpty ? 'text-[var(--color-on-surface-variant)]' : 'text-[var(--color-primary)]'} />
              </div>
              <p className="text-xs text-[var(--color-on-surface-variant)] mt-auto">
                {cashEmpty ?? 'إجمالي الأرصدة'}
              </p>
            </div>
          )}
        </DashboardCard>

        <DashboardCard title="إجمالي المعاملات">
          {transactions.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : transactions.error ? (
            <ErrorState message={transactions.error.message} onRetry={() => transactions.refetch()} />
          ) : (
            <div className="flex flex-col h-full">
              <div className="flex items-center justify-between">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {transactions.data?.count}
                </span>
                <Activity size={18} className="text-[var(--color-primary)]" />
              </div>
              {transactions.data && (
                <p className="text-xs text-[var(--color-on-surface-variant)] mt-auto">
                  {transactions.data.changePercent > 0 ? '+' : ''}
                  {transactions.data.changePercent.toFixed(1)}% منذ الشهر الماضي
                </p>
              )}
            </div>
          )}
        </DashboardCard>
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-3">
        <DashboardCard title="الميزانية مقابل المنفق" className="lg:col-span-2">
          <BudgetVsActualChart />
        </DashboardCard>

        <DashboardCard title="توزيع المصروفات">
          <ExpenseBreakdownChart />
        </DashboardCard>
      </div>

      {/* Second Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-3">
        <DashboardCard title="اتجاه الإنفاق" className="lg:col-span-2">
          <RevenueTrendChart />
        </DashboardCard>

        <DashboardCard title="الموافقات المعلقة">
          <PendingApprovalsList />
        </DashboardCard>
      </div>

      {/* Recent Transactions */}
      <DashboardCard title="آخر المعاملات">
        <DataGrid
          columns={txColumns}
          data={recentTx.data ?? []}
          loading={recentTx.isLoading}
          error={recentTx.error?.message}
          onRetry={recentTx.refetch}
          emptyMessage="لا توجد معاملات حديثة"
          rowKey={(row) => row.id}
        />
      </DashboardCard>
    </Page>
  );
}
