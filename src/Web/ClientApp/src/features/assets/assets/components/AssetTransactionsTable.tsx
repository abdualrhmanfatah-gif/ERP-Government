import { useQuery } from '@tanstack/react-query';
import { api } from '@/shared/api';
import { DataGrid, MoneyDisplay, StatusBadge } from '@/components/ui';
import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';

export interface AssetTransactionSummary {
  id: number;
  documentNumber: string;
  transactionType: string;
  transactionDate: string;
  status: string;
  amount?: number;
  journalEntryId?: number;
  journalEntryNumber?: string;
  isReversal: boolean;
  reversalOfId?: number;
}

const transactionTypeLabels: Record<string, string> = {
  Transfer: 'تحويل',
  Disposal: 'تخلص',
  Revaluation: 'تقييم',
  Impairment: 'إنخفاض',
  Depreciation: 'إهلاك',
};

const transactionStatusRoles: Record<string, BadgeVariant> = {
  Draft: 'draft',
  Approved: 'approved',
  Posting: 'pending',
  Posted: 'closed',
  PostingFailed: 'closed',
};

const transactionStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Approved: 'معتمد',
  Posting: 'قيد الترحيل',
  Posted: 'مُقيد',
  PostingFailed: 'فشل الترحيل',
};

function getTransactionStatusBadge(status: string): { variant: BadgeVariant; label: string } {
  return { variant: transactionStatusRoles[status] ?? 'draft', label: transactionStatusLabels[status] ?? status };
}

export function AssetTransactionsTable({ assetId }: { assetId: number }) {
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['assets', 'transactions', assetId],
    queryFn: async () => {
      return await api.get<AssetTransactionSummary[]>(`/api/Assets/${assetId}/transactions`);
    },
    enabled: assetId > 0,
  });

  return (
    <DataGrid<AssetTransactionSummary>
      data={data ?? []}
      rowKey={(row) => row.id}
      loading={isLoading}
      error={error ? getQueryErrorMessage(error) : undefined}
      onRetry={() => refetch()}
      emptyMessage="لا توجد معاملات"
      columns={[
        {
          id: 'documentNumber',
          accessorKey: 'documentNumber',
          header: 'الرقم',
          width: 130,
          cell: (row) => (
            <span className="flex items-center gap-2">
              <span dir="ltr" className="tabular-nums font-mono">{row.documentNumber}</span>
              {row.isReversal && <span className="text-xs text-[var(--color-error)]">(عكس)</span>}
            </span>
          ),
        },
        { id: 'transactionType', accessorKey: 'transactionType', header: 'النوع', width: 100, cell: (row) => transactionTypeLabels[row.transactionType] ?? row.transactionType },
        { id: 'transactionDate', accessorKey: 'transactionDate', header: 'التاريخ', width: 120, cell: (row) => formatDate(row.transactionDate) },
        {
          id: 'status',
          accessorKey: 'status',
          header: 'الحالة',
          width: 110,
          cell: (row) => {
            const badge = getTransactionStatusBadge(row.status);
            return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>;
          },
        },
        { id: 'amount', accessorKey: 'amount', header: 'المبلغ', align: 'right', cell: (row) => row.amount != null ? <MoneyDisplay value={row.amount} /> : '—' },
        {
          id: 'journalEntryId',
          accessorKey: 'journalEntryId',
          header: 'قيد الترحيل',
          width: 110,
          cell: (row) => row.journalEntryNumber ? (
            <span dir="ltr" className="tabular-nums">{row.journalEntryNumber}</span>
          ) : row.journalEntryId ? (
            <span dir="ltr" className="tabular-nums">{row.journalEntryId}</span>
          ) : (
            '—'
          ),
        },
      ]}
    />
  );
}
