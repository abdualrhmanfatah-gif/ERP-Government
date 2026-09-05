import { useRecentTransactions } from '../hooks/useRecentTransactions';
import { ErrorState } from '../../../components/ui/ErrorState';
import { EmptyState } from '../../../components/ui/EmptyState';
import { StatusBadge } from '../../../components/ui/StatusBadge';

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('ar-SA', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ar-SA', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }).format(date);
}

const statusMap: Record<string, { label: string; variant: 'approved' | 'pending' | 'draft' }> = {
  posted: { label: 'مرحل', variant: 'approved' },
  pending: { label: 'معلق', variant: 'pending' },
  draft: { label: 'مسودة', variant: 'draft' },
};

export function RecentTransactionsTable() {
  const { data, isLoading, error, refetch } = useRecentTransactions();

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-12 bg-[var(--color-surface-container-low)] rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  if (error) {
    return <ErrorState message={error.message} onRetry={refetch} />;
  }

  if (!data || data.length === 0) {
    return <EmptyState message="لا توجد معاملات حديثة" />;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-body-sm">
        <thead>
          <tr className="border-b border-[var(--color-border-container)]">
            <th className="text-start py-3 px-4 font-semibold text-[var(--color-on-surface-variant)]">التاريخ</th>
            <th className="text-start py-3 px-4 font-semibold text-[var(--color-on-surface-variant)]">الوصف</th>
            <th className="text-start py-3 px-4 font-semibold text-[var(--color-on-surface-variant)]">الحساب</th>
            <th className="text-end py-3 px-4 font-semibold text-[var(--color-on-surface-variant)]">المدين</th>
            <th className="text-end py-3 px-4 font-semibold text-[var(--color-on-surface-variant)]">الدائن</th>
            <th className="text-center py-3 px-4 font-semibold text-[var(--color-on-surface-variant)]">الحالة</th>
          </tr>
        </thead>
        <tbody>
          {data.map((transaction) => {
            const status = statusMap[transaction.status];
            return (
              <tr
                key={transaction.id}
                className="border-b border-[var(--color-border-container)] last:border-0 hover:bg-[var(--color-surface-container-low)] transition-colors"
              >
                <td className="py-3 px-4 text-[var(--color-on-surface-variant)]">{formatDate(transaction.date)}</td>
                <td className="py-3 px-4 text-[var(--color-on-surface)] font-medium">{transaction.description}</td>
                <td className="py-3 px-4 text-[var(--color-on-surface-variant)]">{transaction.account}</td>
                <td className="py-3 px-4 text-end tabular-nums">
                  {transaction.debit > 0 ? (
                    <span className="text-[var(--color-error)]">{formatCurrency(transaction.debit)}</span>
                  ) : (
                    <span className="text-[var(--color-on-surface-variant)]">—</span>
                  )}
                </td>
                <td className="py-3 px-4 text-end tabular-nums">
                  {transaction.credit > 0 ? (
                    <span className="text-[var(--color-success)]">{formatCurrency(transaction.credit)}</span>
                  ) : (
                    <span className="text-[var(--color-on-surface-variant)]">—</span>
                  )}
                </td>
                <td className="py-3 px-4 text-center">
                  <StatusBadge variant={status.variant} size="sm">
                    {status.label}
                  </StatusBadge>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
