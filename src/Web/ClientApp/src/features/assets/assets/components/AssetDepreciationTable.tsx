import { DataGrid, MoneyDisplay, StatusBadge } from '@/components/ui';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';
import { useDepreciationSchedules } from '../../asset-depreciation/hooks/useDepreciation';
import { getDepreciationRunStatusBadge } from '../../asset-depreciation/shared/status';
import type { AssetDepreciationSchedule } from '../../asset-depreciation/shared/types';

export function AssetDepreciationTable({ assetId }: { assetId: number }) {
  const { data, isLoading, error, refetch } = useDepreciationSchedules({ assetId });

  return (
    <DataGrid<AssetDepreciationSchedule>
      data={data?.items ?? []}
      rowKey={(row) => row.id}
      loading={isLoading}
      error={error ? getQueryErrorMessage(error) : undefined}
      onRetry={() => refetch()}
      emptyMessage="لا توجد سجلات إهلاك"
      columns={[
        { id: 'depreciationDate', accessorKey: 'depreciationDate', header: 'التاريخ', width: 120, cell: (row) => formatDate(row.depreciationDate) },
        { id: 'fiscalYear', accessorKey: 'fiscalYear', header: 'السنة المالية' },
        { id: 'periodNumber', accessorKey: 'periodNumber', header: 'الفترة', width: 80 },
        { id: 'amount', accessorKey: 'amount', header: 'إهلاك الفترة', align: 'right', cell: (row) => <MoneyDisplay value={row.amount} /> },
        { id: 'accumulatedDepreciation', accessorKey: 'accumulatedDepreciation', header: 'مجمع الإهلاك الختامي', align: 'right', cell: (row) => <MoneyDisplay value={row.accumulatedDepreciation} /> },
        {
          id: 'status',
          accessorKey: 'status',
          header: 'الحالة',
          width: 110,
          cell: (row) => {
            const badge = getDepreciationRunStatusBadge(row.status);
            return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>;
          },
        },
        { id: 'journalEntryId', accessorKey: 'journalEntryId', header: 'قيد الترحيل', width: 110, cell: (row) => row.journalEntryId ? <span dir="ltr" className="tabular-nums">{row.journalEntryId}</span> : '—' },
      ]}
    />
  );
}
