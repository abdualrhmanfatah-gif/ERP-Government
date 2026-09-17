import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, DataGrid, StatusBadge, MoneyDisplay } from '@/components/ui';
import { useImpairmentsList } from '../hooks/useImpairments';
import { impairmentStatusOptions, type Impairment } from '../shared/types';
import { getImpairmentStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';

export function ImpairmentsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading, isFetching, error, refetch } = useImpairmentsList({ search: search || undefined, status: status || undefined, page });
  const items = useMemo(() => data?.items ?? [], [data]);
  const pageSize = data?.pageSize ?? 20;

  return (
    <Page title="الإنخفاض" description="إدارة إنخفاض قيمة الأصول" maxWidth="full" loading={isLoading && !data}
      actions={<Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/assets/impairments/create')}>إنخفاض جديد</Button>}
      toolbar={<FilterBar hasFilters={!!search || !!status} onClear={() => { setSearch(''); setStatus(''); setPage(1); }}>
        <FilterSearch value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="بحث..." className="flex-1 min-w-48" />
        <FilterSelect label="الحالة" value={status} onChange={(v) => { setStatus(v); setPage(1); }} options={impairmentStatusOptions} />
      </FilterBar>}
    >
      <DataGrid<Impairment> data={items} rowKey={(r) => r.id} onRowClick={(r) => navigate(`/assets/impairments/${r.id}`)}
        loading={isFetching && !isLoading} error={error ? getQueryErrorMessage(error) : undefined} onRetry={() => refetch()}
        emptyMessage="لا توجد إنخفاضات" pagination={{ pageIndex: page - 1, pageSize }} totalItems={data?.totalCount ?? 0} onPageChange={(i) => setPage(i + 1)}
        columns={[
          { id: 'documentNumber', accessorKey: 'documentNumber', header: 'الرقم', width: 120, cell: (r) => <span dir="ltr" className="tabular-nums font-mono">{r.documentNumber}</span> },
          { id: 'assetCode', accessorKey: 'assetCode', header: 'كود الأصل', width: 120 },
          { id: 'assetName', accessorKey: 'assetName', header: 'الأصل' },
          { id: 'impairmentDate', accessorKey: 'impairmentDate', header: 'التاريخ', width: 120, cell: (r) => formatDate(r.impairmentDate) },
          { id: 'impairmentAmount', accessorKey: 'impairmentAmount', header: 'المبلغ', align: 'right', cell: (r) => <MoneyDisplay value={r.impairmentAmount} /> },
          { id: 'isReversal', accessorKey: 'isReversal', header: 'عكس', width: 60, cell: (r) => r.isReversal ? 'نعم' : 'لا' },
          { id: 'status', accessorKey: 'status', header: 'الحالة', width: 110, cell: (r) => { const badge = getImpairmentStatusBadge(r.status); return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>; } },
        ]}
      />
    </Page>
  );
}
