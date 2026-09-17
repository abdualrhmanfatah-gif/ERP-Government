import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, DataGrid, MoneyDisplay, StatusBadge } from '@/components/ui';
import { useAssetsList } from '../hooks/useAssets';
import { assetStatusLabels, type Asset } from '../shared/types';
import { getAssetStatusBadge } from '../../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';

const DEFAULT_STATUS = 'Active';

export function AssetsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState(DEFAULT_STATUS);
  const [page, setPage] = useState(1);

  const { data, isLoading, isFetching, error, refetch } = useAssetsList({
    search: search || undefined,
    status: status === 'All' ? undefined : status,
    page,
  });

  const items = useMemo(() => data?.items ?? [], [data]);
  const pageSize = data?.pageSize ?? 20;
  const hasFilters = !!search || status !== DEFAULT_STATUS;
  const isInitialLoading = isLoading && !data;

  const statusOptions = useMemo(
    () => Object.entries(assetStatusLabels).map(([value, label]) => ({ value, label })),
    []
  );

  const clearFilters = () => {
    setSearch('');
    setStatus(DEFAULT_STATUS);
    setPage(1);
  };

  return (
    <Page
      title="سجل الأصول"
      description="عرض وإدارة الأصول الثابتة"
      maxWidth="full"
      loading={isInitialLoading}
      actions={
        <Button
          variant="primary"
          size="sm"
          icon={<Plus size={16} />}
          onClick={() => navigate('/assets/create')}
        >
          إضافة أصل
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={clearFilters}>
          <FilterSearch
            value={search}
            onChange={(v) => { setSearch(v); setPage(1); }}
            placeholder="بحث بالكود، الاسم، الوسم، الباركود، التسلسلي..."
            className="flex-1 min-w-48"
          />
          <FilterSelect
            label="الحالة"
            value={status}
            onChange={(v) => { setStatus(v || DEFAULT_STATUS); setPage(1); }}
            options={statusOptions}
          />
        </FilterBar>
      }
    >
      <DataGrid<Asset>
        data={items}
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/assets/${row.id}`)}
        loading={isFetching && !isInitialLoading}
        error={error ? getQueryErrorMessage(error) : undefined}
        onRetry={() => refetch()}
        emptyMessage={hasFilters ? 'لا توجد نتائج مطابقة لمعايير البحث' : 'لا توجد أصول'}
        pagination={{ pageIndex: page - 1, pageSize }}
        totalItems={data?.totalCount ?? 0}
        onPageChange={(pageIndex) => setPage(pageIndex + 1)}
        columns={[
          {
            id: 'code',
            accessorKey: 'code',
            header: 'الكود',
            width: 120,
            cell: (row) => <span dir="ltr" className="tabular-nums font-mono">{row.code}</span>,
          },
          { id: 'name', accessorKey: 'name', header: 'الاسم' },
          { id: 'assetGroupName', accessorKey: 'assetGroupName', header: 'المجموعة' },
          {
            id: 'status',
            accessorKey: 'status',
            header: 'الحالة',
            width: 130,
            cell: (row) => {
              const badge = getAssetStatusBadge(row.status);
              return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>;
            },
          },
          {
            id: 'originalValue',
            accessorKey: 'originalValue',
            header: 'القيمة الأصلية',
            align: 'right',
            cell: (row) => <MoneyDisplay value={row.originalValue} />,
          },
          {
            id: 'assetTag',
            accessorKey: 'assetTag',
            header: 'الوسم',
            cell: (row) => <span dir="ltr" className="tabular-nums">{row.assetTag ?? '—'}</span>,
          },
        ]}
      />
    </Page>
  );
}
