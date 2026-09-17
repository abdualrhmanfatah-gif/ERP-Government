import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, DataGrid, StatusBadge } from '@/components/ui';
import { useTransfersList } from '../hooks/useTransfers';
import { transferStatusOptions, type TransferListItem } from '../shared/types';
import { getTransferStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';

export function TransfersListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading, isFetching, error, refetch } = useTransfersList({
    search: search || undefined,
    status: status || undefined,
    page,
  });

  const items = useMemo(() => data?.items ?? [], [data]);
  const pageSize = data?.pageSize ?? 20;
  const hasFilters = !!search || !!status;

  return (
    <Page
      title="نقل الأصول"
      description="إدارة نقل الأصول بين المواقع والحرّاس"
      maxWidth="full"
      loading={isLoading && !data}
      actions={
        <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/assets/transfers/create')}>
          نقل جديد
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={() => { setSearch(''); setStatus(''); setPage(1); }}>
          <FilterSearch value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="بحث بالرقم أو الأصل..." className="flex-1 min-w-48" />
          <FilterSelect label="الحالة" value={status} onChange={(v) => { setStatus(v); setPage(1); }} options={transferStatusOptions} />
        </FilterBar>
      }
    >
      <DataGrid<TransferListItem>
        data={items}
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/assets/transfers/${row.id}`)}
        loading={isFetching && !isLoading}
        error={error ? getQueryErrorMessage(error) : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد عمليات نقل"
        pagination={{ pageIndex: page - 1, pageSize }}
        totalItems={data?.totalCount ?? 0}
        onPageChange={(pageIndex) => setPage(pageIndex + 1)}
        columns={[
          { id: 'documentNumber', accessorKey: 'documentNumber', header: 'الرقم', width: 120, cell: (row) => <span dir="ltr" className="tabular-nums font-mono">{row.documentNumber}</span> },
          { id: 'assetCode', accessorKey: 'assetCode', header: 'كود الأصل', width: 120, cell: (row) => <span dir="ltr" className="tabular-nums">{row.assetCode}</span> },
          { id: 'assetName', accessorKey: 'assetName', header: 'الأصل' },
          { id: 'transactionDate', accessorKey: 'transactionDate', header: 'التاريخ', width: 120, cell: (row) => formatDate(row.transactionDate) },
          { id: 'fromLocationName', accessorKey: 'fromLocationName', header: 'من موقع', cell: (row) => row.fromLocationName ?? '—' },
          { id: 'toLocationName', accessorKey: 'toLocationName', header: 'إلى موقع', cell: (row) => row.toLocationName ?? '—' },
          {
            id: 'status', accessorKey: 'status', header: 'الحالة', width: 110,
            cell: (row) => {
              const badge = getTransferStatusBadge(row.status);
              return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>;
            },
          },
        ]}
      />
    </Page>
  );
}
