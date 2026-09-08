import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePaymentOrdersList } from '../hooks/usePaymentOrders';
import {
  paymentOrderStatusLabels,
  budgetCheckStatusLabels,
  PaymentOrderStatus,
} from '../shared/types';
import { PaymentsStatusBadge } from '@/components/PaymentsStatusBadge';
import { Page, Button } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { FilterSelect } from '@/components/ui/FilterSelect';
import { Plus } from 'lucide-react';
import type { PaymentOrderDto } from '../../../../web-api-client';

const statusOptions = Object.entries(paymentOrderStatusLabels).map(([value, label]) => ({ value, label }));
const budgetCheckOptions = Object.entries(budgetCheckStatusLabels).map(([value, label]) => ({ value, label }));

export function PaymentOrdersListPage() {
  const navigate = useNavigate();

  const [statusFilter, setStatusFilter] = useState<string>('');
  const [budgetCheckFilter, setBudgetCheckFilter] = useState<string>('');

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
    { header: 'التاريخ', cell: (row) => <span className="text-sm">{row.paymentOrderDate ? new Date(row.paymentOrderDate).toLocaleDateString('ar-EG') : '—'}</span> },
    { header: 'المبلغ الصافي', cell: (row) => <span className="font-mono">{((row.amountGross ?? 0) - (row.deductionAmount ?? 0)).toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span> },
    {
      header: 'الحالة',
      cell: (row) => <PaymentsStatusBadge status={row.status} variant="order" />,
    },
    {
      header: 'فحص الميزانية',
      cell: (row) => <PaymentsStatusBadge status={row.budgetCheckStatus} variant="budgetCheck" />,
    },
    { header: 'الصندوق', cell: (row) => <span className="text-sm">{row.fundId}</span> },
  ];

  const hasFilters = !!statusFilter || !!budgetCheckFilter;

  return (
    <Page
      title="أوامر الدفع"
      loading={isLoading}
      actions={
        <Button variant="primary" size="sm" onClick={() => navigate('/payments/payment-orders/create')}>
          <Plus size={16} className="ms-1" />
          أمر دفع جديد
        </Button>
      }
      toolbar={
        <div className="flex gap-2 items-center">
          <FilterSelect
            value={statusFilter}
            onChange={setStatusFilter}
            options={statusOptions}
            placeholder="الحالة"
            label="الحالة"
          />
          <FilterSelect
            value={budgetCheckFilter}
            onChange={setBudgetCheckFilter}
            options={budgetCheckOptions}
            placeholder="فحص الميزانية"
            label="فحص الميزانية"
          />
          {hasFilters && (
            <Button variant="ghost" size="sm" onClick={() => { setStatusFilter(''); setBudgetCheckFilter(''); }}>
              مسح الفلاتر
            </Button>
          )}
        </div>
      }
    >
      <DataGrid
        columns={columns}
        data={orders ?? []}
        loading={isLoading}
        emptyMessage="لا توجد أوامر دفع بعد"
        rowKey={(row) => row.id}
      />
    </Page>
  );
}
