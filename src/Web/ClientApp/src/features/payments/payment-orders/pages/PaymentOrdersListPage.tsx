import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePaymentOrdersList } from '../hooks/usePaymentOrders';
import {
  paymentOrderStatusLabels,
  PaymentOrderStatus,
} from '../shared/types';
import { PaymentsStatusBadge } from '@/components/PaymentsStatusBadge';
import { Page, Button, FilterBar, MoneyDisplay } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { FilterSelect } from '@/components/ui/FilterSelect';
import { Plus } from 'lucide-react';
import type { PaymentOrderDto } from '../../../../web-api-client';

const statusOptions = Object.entries(paymentOrderStatusLabels).map(([value, label]) => ({ value, label }));

export function PaymentOrdersListPage() {
  const navigate = useNavigate();

  const [statusFilter, setStatusFilter] = useState<string>('');

  const { data: orders, isLoading } = usePaymentOrdersList({
    status: statusFilter ? (statusFilter as PaymentOrderStatus) : undefined,
  });

  const columns: DataGridColumn<PaymentOrderDto>[] = [
    {
      header: 'رقم أمر الدفع',
      cell: (row) => (
        <Button
          variant="link"
          className="font-mono"
          onClick={() => navigate(`/payments/payment-orders/${row.id}`)}
        >
          {row.paymentOrderNumber}
        </Button>
      ),
    },
    { header: 'المورد', cell: (row) => <span className="text-sm">{row.beneficiaryName}</span> },
    { header: 'التاريخ', cell: (row) => <span className="text-sm">{row.paymentOrderDate ? new Date(row.paymentOrderDate).toLocaleDateString('ar-YE') : '—'}</span> },
    { header: 'المبلغ الصافي', cell: (row) => <MoneyDisplay value={(row.amountGross ?? 0) - (row.deductionAmount ?? 0)} /> },
    {
      header: 'الحالة',
      cell: (row) => <PaymentsStatusBadge status={row.status} variant="order" />,
    },
    { header: 'الصندوق', cell: (row) => <span className="text-sm">{row.fundId}</span> },
  ];

  const hasFilters = !!statusFilter;

  return (
    <Page
      title="أوامر الدفع"
      actions={
        <Button variant="primary" size="sm" onClick={() => navigate('/payments/payment-orders/create')}>
          <Plus size={16} className="ms-1" />
          أمر دفع جديد
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={() => setStatusFilter('')}>
          <FilterSelect
            value={statusFilter}
            onChange={setStatusFilter}
            options={statusOptions}
            placeholder="الحالة"
            label="الحالة"
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={orders ?? []}
        loading={isLoading}
        emptyMessage="لا توجد أوامر دفع بعد"
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/payments/payment-orders/${row.id}`)}
      />
    </Page>
  );
}
