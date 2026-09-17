import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, DataGrid, StatusBadge, MoneyDisplay } from '@/components/ui';
import { useRevaluationsList } from '../hooks/useRevaluations';
import { revaluationStatusOptions, type Revaluation } from '../shared/types';
import { getRevaluationStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';

export function RevaluationsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading, isFetching, error, refetch } = useRevaluationsList({ search: search || undefined, status: status || undefined, page });
  const items = useMemo(() => data?.items ?? [], [data]);
  const pageSize = data?.pageSize ?? 20;

  return (
    <Page title="التقييمات" description="إدارة إعادة تقييم الأصول" maxWidth="full" loading={isLoading && !data}
      actions={<Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/assets/revaluations/create')}>تقييم جديد</Button>}
      toolbar={<FilterBar hasFilters={!!search || !!status} onClear={() => { setSearch(''); setStatus(''); setPage(1); }}>
        <FilterSearch value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="بحث..." className="flex-1 min-w-48" />
        <FilterSelect label="الحالة" value={status} onChange={(v) => { setStatus(v); setPage(1); }} options={revaluationStatusOptions} />
      </FilterBar>}
    >
      <DataGrid<Revaluation> data={items} rowKey={(r) => r.id} onRowClick={(r) => navigate(`/assets/revaluations/${r.id}`)}
        loading={isFetching && !isLoading} error={error ? getQueryErrorMessage(error) : undefined} onRetry={() => refetch()}
        emptyMessage="لا توجد تقييمات" pagination={{ pageIndex: page - 1, pageSize }} totalItems={data?.totalCount ?? 0} onPageChange={(i) => setPage(i + 1)}
        columns={[
          { id: 'documentNumber', accessorKey: 'documentNumber', header: 'الرقم', width: 120, cell: (r) => <span dir="ltr" className="tabular-nums font-mono">{r.documentNumber}</span> },
          { id: 'assetCode', accessorKey: 'assetCode', header: 'كود الأصل', width: 120 },
          { id: 'assetName', accessorKey: 'assetName', header: 'الأصل' },
          { id: 'revaluationDate', accessorKey: 'revaluationDate', header: 'التاريخ', width: 120, cell: (r) => formatDate(r.revaluationDate) },
          { id: 'revaluationAmount', accessorKey: 'revaluationAmount', header: 'المبلغ', align: 'right', cell: (r) => <MoneyDisplay value={r.revaluationAmount} /> },
          { id: 'type', accessorKey: 'type', header: 'النوع', width: 80 },
          { id: 'status', accessorKey: 'status', header: 'الحالة', width: 110, cell: (r) => { const badge = getRevaluationStatusBadge(r.status); return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>; } },
        ]}
      />
    </Page>
  );
}
