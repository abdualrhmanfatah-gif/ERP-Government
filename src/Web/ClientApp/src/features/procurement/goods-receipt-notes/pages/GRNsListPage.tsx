import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, Skeleton, ErrorState } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Eye, Plus } from 'lucide-react';
import { grnStatusLabels, grnStatusVariant } from '../shared/types';
import type { GRN, GRNStatus } from '../shared/types';
import { useGRNsList } from '../hooks/useGRNs';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { usePermission } from '@/shared/hooks/usePermission';
import { PROCUREMENT_PERMISSIONS } from '@/shared/constants/permissions';

const statusOptions = Object.entries(grnStatusLabels).map(([value, label]) => ({ value, label }));

export default function GRNsListPage() {
  const navigate = useNavigate();
  const { hasPermission: canCreate } = usePermission(PROCUREMENT_PERMISSIONS.GoodsReceipts.Create);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [page, setPage] = useState(1);

  const { data: response, isLoading, isError } = useGRNsList({
    search: search || undefined,
    status: statusFilter || undefined,
    page,
    pageSize: 20,
  });

  const items = response?.items ?? [];
  const totalCount = response?.totalCount ?? 0;

  const columns: DataGridColumn<GRN>[] = useMemo(() => [
    { key: 'grnNumber', header: 'رقم إشعار الاستلام', width: '150px' },
    { key: 'purchaseOrderNumber', header: 'رقم أمر الشراء', width: '120px' },
    { key: 'supplierName', header: 'المورد', width: '150px' },
    { key: 'warehouseName', header: 'المستودع', width: '120px' },
    {
      key: 'status',
      header: 'الحالة',
      width: '120px',
      render: (v) => (
        <StatusBadge
          status={v as GRNStatus}
          labels={grnStatusLabels}
          variants={grnStatusVariant}
        />
      ),
    },
    { key: 'created', header: 'تاريخ الإنشاء', width: '120px', render: (v) => new Date(v).toLocaleDateString('ar-YE') },
    {
      key: 'actions',
      header: 'إجراءات',
      width: '80px',
      render: (_, row) => (
        <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/goods-receipt-notes/${row.id}`)}>
          <Eye className="h-4 w-4" />
        </Button>
      ),
    },
  ], [navigate]);

  function handleClearFilters() {
    setSearch('');
    setStatusFilter('');
    setPage(1);
  }

  const hasFilters = !!search || !!statusFilter;

  if (isError) return <ErrorState message="خطأ في تحميل إشعارات الاستلام" />;

  return (
    <Page
      title="إشعارات الاستلام"
      actions={canCreate ? (
        <Button onClick={() => navigate('/procurement/goods-receipt-notes/create')}>
          <Plus className="h-4 w-4 ms-1" />إشعار استلام جديد
        </Button>
      ) : undefined}
    >
      <FilterBar onClear={handleClearFilters} hasFilters={hasFilters}>
        <FilterSearch value={search} onChange={setSearch} placeholder="بحث برقم الإشعار..." />
        <FilterSelect value={statusFilter} onChange={setStatusFilter} options={statusOptions} placeholder="الحالة" />
      </FilterBar>

      <div className="mt-4">
        {isLoading ? (
          <Skeleton className="h-96" />
        ) : (
          <DataGrid
            data={items}
            columns={columns}
            emptyMessage="لا توجد إشعارات استلام"
            pagination={{
              page,
              pageSize: 20,
              totalCount,
              onPageChange: setPage,
            }}
          />
        )}
      </div>
    </Page>
  );
}
