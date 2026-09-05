import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { TrendingUp, Clock, Wallet, Plus, Activity } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Loading } from '../../../components/ui/Loading';
import { ErrorState } from '../../../components/ui/ErrorState';
import { EmptyState } from '../../../components/ui/EmptyState';
import { DashboardCard } from '../components/DashboardCard';
import { BudgetVsActualChart } from '../components/BudgetVsActualChart';
import { RevenueTrendChart } from '../components/RevenueTrendChart';
import { ExpenseBreakdownChart } from '../components/ExpenseBreakdownChart';
import { RecentTransactionsTable } from '../components/RecentTransactionsTable';
import { PendingApprovalsList } from '../components/PendingApprovalsList';
import { useBudgetUtilization } from '../hooks/useBudgetUtilization';
import { usePendingApprovalsCount } from '../hooks/usePendingApprovalsCount';
import { useCashPosition } from '../hooks/useCashPosition';
import { useTotalTransactions } from '../hooks/useTotalTransactions';

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('ar-SA', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

export function DashboardPage() {
  const navigate = useNavigate();
  const budget = useBudgetUtilization();
  const approvals = usePendingApprovalsCount();
  const cash = useCashPosition();
  const transactions = useTotalTransactions();

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
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-headline-sm font-bold text-[var(--color-on-surface)]">لوحة التحكم</h1>
        <Button variant="primary" icon={<Plus size={16} />} onClick={() => navigate('/accounting/journal-entries/new')}>
          إجراء سريع
        </Button>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <DashboardCard
          title="نسبة تنفيذ الميزانية"
          onClick={() => navigate('/budget/budgets')}
          className="p-6"
        >
          {budget.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : budget.error ? (
            <ErrorState message={budget.error.message} onRetry={() => budget.refetch()} />
          ) : budgetEmpty ? (
            <EmptyState message={budgetEmpty} />
          ) : (
            <>
              <div className="flex items-center justify-between mb-2">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {budgetValue}
                </span>
                <TrendingUp size={20} className="text-[var(--color-primary)]" />
              </div>
              {budget.data && (
                <p className="text-xs text-[var(--color-on-surface-variant)]">
                  {formatCurrency(budget.data.totalSpent)} / {formatCurrency(budget.data.totalAllocated)}
                </p>
              )}
            </>
          )}
        </DashboardCard>

        <DashboardCard
          title="الموافقات المعلقة"
          onClick={() => navigate('/approval-rules/pending')}
          className="p-6"
        >
          {approvals.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : approvals.error ? (
            <ErrorState message={approvals.error.message} onRetry={() => approvals.refetch()} />
          ) : approvals.data?.count === 0 ? (
            <EmptyState message="لا توجد موافقات معلقة" />
          ) : (
            <>
              <div className="flex items-center justify-between mb-2">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {approvals.data?.count}
                </span>
                <Clock size={20} className="text-[var(--color-primary)]" />
              </div>
              {approvals.data && approvals.data.count > 0 && (
                <p className="text-xs text-[var(--color-on-surface-variant)]">موافقة معلقة</p>
              )}
            </>
          )}
        </DashboardCard>

        <DashboardCard
          title="الموقف النقدي"
          onClick={() => navigate('/banking/bank-accounts')}
          className="p-6"
        >
          {cash.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : cash.error ? (
            <ErrorState message={cash.error.message} onRetry={() => cash.refetch()} />
          ) : cashEmpty ? (
            <EmptyState message={cashEmpty} />
          ) : (
            <>
              <div className="flex items-center justify-between mb-2">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {cash.data?.hasBankAccounts ? formatCurrency(cash.data.totalBalance) : '—'}
                </span>
                <Wallet size={20} className="text-[var(--color-primary)]" />
              </div>
              {cash.data?.hasBankAccounts && (
                <p className="text-xs text-[var(--color-on-surface-variant)]">إجمالي الأرصدة</p>
              )}
            </>
          )}
        </DashboardCard>

        <DashboardCard title="إجمالي المعاملات" className="p-6">
          {transactions.isLoading ? (
            <Loading text="جاري التحميل…" />
          ) : transactions.error ? (
            <ErrorState message={transactions.error.message} onRetry={() => transactions.refetch()} />
          ) : (
            <>
              <div className="flex items-center justify-between mb-2">
                <span className="text-headline-lg font-bold text-[var(--color-on-surface)] tabular-nums">
                  {transactions.data?.count}
                </span>
                <Activity size={20} className="text-[var(--color-primary)]" />
              </div>
              {transactions.data && (
                <p className="text-xs text-[var(--color-on-surface-variant)]">
                  {transactions.data.changePercent > 0 ? '+' : ''}
                  {transactions.data.changePercent.toFixed(1)}% منذ الشهر الماضي
                </p>
              )}
            </>
          )}
        </DashboardCard>
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <DashboardCard title="الميزانية مقابل المنفق" className="lg:col-span-2">
          <BudgetVsActualChart />
        </DashboardCard>

        <DashboardCard title="توزيع المصروفات">
          <ExpenseBreakdownChart />
        </DashboardCard>
      </div>

      {/* Second Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <DashboardCard title="اتجاه الإنفاق" className="lg:col-span-2">
          <RevenueTrendChart />
        </DashboardCard>

        <DashboardCard title="الموافقات المعلقة">
          <PendingApprovalsList />
        </DashboardCard>
      </div>

      {/* Recent Transactions */}
      <DashboardCard title="آخر المعاملات">
        <RecentTransactionsTable />
      </DashboardCard>
    </div>
  );
}
