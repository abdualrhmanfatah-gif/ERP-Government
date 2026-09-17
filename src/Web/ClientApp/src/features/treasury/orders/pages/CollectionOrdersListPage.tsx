import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, FilterBar, FilterSelect, Badge, EmptyState, MoneyDisplay, Page } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { CollectionOrderStatus } from '@/web-api-client';
import { formatDate } from '@/shared/utils/formatters';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { useCollectionOrders } from '../../hooks/useCollectionOrders';
import { collectionOrderStatusLabels, collectionOrderStatusBadgeVariant } from '../../shared/types';
import type { CollectionOrderDto } from '../../shared/types';

export default function CollectionOrdersListPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState('');

  const { data: orders = [], isLoading, isError, error, refetch } = useCollectionOrders({
    status: statusFilter ? (statusFilter as CollectionOrderStatus) : undefined,
  });

  const columns: DataGridColumn<CollectionOrderDto>[] = useMemo(
    () => [
      {
        key: 'orderNumber',
        header: 'رقم الأمر',
        render: (row) => <span className="font-medium tabular-nums">{row.orderNumber}</span>,
      },
      {
        key: 'orderDate',
        header: 'التاريخ',
        render: (row) => formatDate(row.orderDate),
      },
      {
        key: 'revenueClaimNumber',
        header: 'رقم المطالبة',
        render: (row) => (
          <span className="tabular-nums text-[var(--color-primary)]">
            {row.revenueClaimNumber ?? '—'}
          </span>
        ),
      },
      {
        key: 'authorizedAmount',
        header: 'المبلغ المفوض',
        render: (row) => <MoneyDisplay value={row.authorizedAmount ?? 0} />,
      },
      {
        key: 'collectedAmount',
        header: 'المحصّل',
        render: (row) => <MoneyDisplay value={row.collectedAmount ?? 0} />,
      },
      {
        key: 'availableAmount',
        header: 'المتاح',
        render: (row) => <MoneyDisplay value={row.availableAmount ?? 0} />,
      },
      {
        key: 'status',
        header: 'الحالة',
        render: (row) => (
          <Badge variant={collectionOrderStatusBadgeVariant[row.status ?? 'Draft']}>
            {collectionOrderStatusLabels[row.status ?? 'Draft']}
          </Badge>
        ),
      },
      {
        key: 'actions',
        header: '',
        render: (row) => (
          <Button
            variant="ghost"
            size="icon"
            aria-label={`عرض الأمر ${row.orderNumber}`}
            onClick={() => navigate(`/treasury/collection-orders/${row.id}`)}
          >
            <Eye size={16} />
          </Button>
        ),
      },
    ],
    [navigate],
  );

  return (
    <Page
      title="أوامر التحصيل"
      actions={
        <Button onClick={() => navigate('/treasury/collection-orders/create')}>
          <Plus size={16} />
          أمر تحصيل جديد
        </Button>
      }
      toolbar={
        <FilterBar
          hasFilters={!!statusFilter}
          onClear={() => setStatusFilter('')}
        >
          <FilterSelect
            label="الحالة"
            value={statusFilter}
            onChange={setStatusFilter}
            options={[
              { value: '', label: 'الكل' },
              ...Object.entries(collectionOrderStatusLabels).map(([value, label]) => ({ value, label })),
            ]}
          />
        </FilterBar>
      }
      loading={isLoading}
    >
      {isError ? (
        <div className="p-4 text-center flex flex-col items-center gap-2">
          <p className="text-sm text-[var(--color-error)]">{getQueryErrorMessage(error)}</p>
          <Button variant="outline" size="sm" onClick={() => refetch()}>إعادة المحاولة</Button>
        </div>
      ) : orders.length === 0 ? (
        <EmptyState message="لا توجد أوامر تحصيل" />
      ) : (
        <DataGrid columns={columns} data={orders} rowKey={(row) => row.id ?? 0} />
      )}
    </Page>
  );
}
