import { useMemo, useState } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, Skeleton, ErrorState } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Eye, Plus } from 'lucide-react';
import { supplierInvoiceStatusLabels, supplierInvoiceStatusVariant } from '../shared/types';
import type { SupplierInvoice, SupplierInvoiceStatus } from '../shared/types';
import { useSupplierInvoicesList } from '../hooks/useSupplierInvoices';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { usePermission } from '@/shared/hooks/usePermission';
import { PROCUREMENT_PERMISSIONS } from '@/shared/constants/permissions';

const statusOptions = Object.entries(supplierInvoiceStatusLabels).map(([value, label]) => ({ value, label }));

export default function SupplierInvoicesListPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { hasPermission: canCreate } = usePermission(PROCUREMENT_PERMISSIONS.SupplierInvoices.Create);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [purchaseOrderIdFilter] = useState<number | undefined>(
    searchParams.get('purchaseOrderId') ? Number(searchParams.get('purchaseOrderId')) : undefined,
  );
  const [page, setPage] = useState(1);

  const { data: response, isLoading, isError } = useSupplierInvoicesList({
    search: search || undefined,
    status: statusFilter || undefined,
    purchaseOrderId: purchaseOrderIdFilter,
    page,
    pageSize: 20,
  });

  const items = response?.items ?? [];
  const totalCount = response?.totalCount ?? 0;

  const columns: DataGridColumn<SupplierInvoice>[] = useMemo(() => [
    { key: 'invoiceNumber', header: 'رقم الفاتورة', width: '150px' },
    { key: 'supplierInvoiceNumber', header: 'رقم فاتورة المورد', width: '150px' },
    {
      key: 'purchaseOrderNumber',
      header: 'رقم أمر الشراء',
      width: '120px',
      render: (v, row) => (
        <Link
          to={`/procurement/purchase-orders/${row.purchaseOrderId}`}
          className="text-primary underline hover:no-underline"
          dir="ltr"
        >
          {v || `#${row.purchaseOrderId}`}
        </Link>
      ),
    },
    { key: 'invoiceDate', header: 'التاريخ', width: '120px', render: (v) => new Date(v).toLocaleDateString('ar-YE') },
    {
      key: 'status',
      header: 'الحالة',
      width: '120px',
      render: (v) => (
        <StatusBadge
          status={v as SupplierInvoiceStatus}
          labels={supplierInvoiceStatusLabels}
          variants={supplierInvoiceStatusVariant}
        />
      ),
    },
    { key: 'grandTotal', header: 'الإجمالي', width: '150px', render: (v) => v ? `${v.toLocaleString('ar-YE')} ر.ي` : '-' },
    { key: 'created', header: 'تاريخ الإنشاء', width: '120px', render: (v) => new Date(v).toLocaleDateString('ar-YE') },
    {
      key: 'actions',
      header: 'إجراءات',
      width: '80px',
      render: (_, row) => (
        <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/supplier-invoices/${row.id}`)}>
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

  const hasFilters = !!search || !!statusFilter || !!purchaseOrderIdFilter;

  if (isError) return <ErrorState message="خطأ في تحميل فواتير الموردين" />;

  return (
    <Page
      title="فواتير الموردين"
      actions={canCreate ? (
        <Button onClick={() => navigate('/procurement/supplier-invoices/create')}>
          <Plus className="h-4 w-4 ms-1" />فاتورة جديدة
        </Button>
      ) : undefined}
    >
      <FilterBar onClear={handleClearFilters} hasFilters={hasFilters}>
        <FilterSearch value={search} onChange={setSearch} placeholder="بحث برقم الفاتورة..." />
        <FilterSelect value={statusFilter} onChange={setStatusFilter} options={statusOptions} placeholder="الحالة" />
      </FilterBar>

      <div className="mt-4">
        {isLoading ? (
          <Skeleton className="h-96" />
        ) : (
          <DataGrid
            data={items}
            columns={columns}
            emptyMessage="لا توجد فواتير موردين"
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
