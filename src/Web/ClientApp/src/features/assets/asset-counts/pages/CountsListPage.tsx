import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, DataGrid, StatusBadge } from '@/components/ui';
import { useCountsList } from '../hooks/useCounts';
import { countStatusOptions, type AssetCount } from '../shared/types';
import { getCountStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';

export function CountsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading, isFetching, error, refetch } = useCountsList({ search: search || undefined, status: status || undefined, page });
  const items = useMemo(() => data?.items ?? [], [data]);
  const pageSize = data?.pageSize ?? 20;

  return (
    <Page title="الجرد" description="إدارة جرد الأصول" maxWidth="full" loading={isLoading && !data}
      actions={<Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/assets/counts/create')}>جرد جديد</Button>}
      toolbar={<FilterBar hasFilters={!!search || !!status} onClear={() => { setSearch(''); setStatus(''); setPage(1); }}>
        <FilterSearch value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="بحث..." className="flex-1 min-w-48" />
        <FilterSelect label="الحالة" value={status} onChange={(v) => { setStatus(v); setPage(1); }} options={countStatusOptions} />
      </FilterBar>}
    >
      <DataGrid<AssetCount> data={items} rowKey={(r) => r.id} onRowClick={(r) => navigate(`/assets/counts/${r.id}`)}
        loading={isFetching && !isLoading} error={error ? getQueryErrorMessage(error) : undefined} onRetry={() => refetch()}
        emptyMessage="لا توجد جرود" pagination={{ pageIndex: page - 1, pageSize }} totalItems={data?.totalCount ?? 0} onPageChange={(i) => setPage(i + 1)}
        columns={[
          { id: 'documentNumber', accessorKey: 'documentNumber', header: 'الرقم', width: 120, cell: (r) => <span dir="ltr" className="tabular-nums font-mono">{r.documentNumber}</span> },
          { id: 'countDate', accessorKey: 'countDate', header: 'التاريخ', width: 120, cell: (r) => formatDate(r.countDate) },
          { id: 'resolvedScopeLabel', accessorKey: 'resolvedScopeLabel', header: 'النطاق' },
          { id: 'totalAssets', accessorKey: 'totalAssets', header: 'الإجمالي', width: 80, align: 'right' },
          { id: 'foundCount', accessorKey: 'foundCount', header: 'موجود', width: 80, align: 'right' },
          { id: 'notFoundCount', accessorKey: 'notFoundCount', header: 'غير موجود', width: 80, align: 'right' },
          { id: 'notExaminedCount', accessorKey: 'notExaminedCount', header: 'لم يفحص', width: 80, align: 'right' },
          { id: 'status', accessorKey: 'status', header: 'الحالة', width: 110, cell: (r) => { const badge = getCountStatusBadge(r.status); return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>; } },
        ]}
      />
    </Page>
  );
}
