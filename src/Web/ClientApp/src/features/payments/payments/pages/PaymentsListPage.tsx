import { useState } from 'react';
import { usePaymentsList } from '../hooks/usePayments';
import {
  paymentMethodLabels,
  paymentStatusLabels,
  PaymentMethod,
  PaymentStatus,
} from '../shared/types';
import { Page, Button, FilterBar, MoneyDisplay } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { FilterSelect } from '@/components/ui/FilterSelect';
import { PaymentsStatusBadge } from '@/components/PaymentsStatusBadge';
import type { PaymentDto } from '../../../../web-api-client';

const statusOptions = Object.entries(paymentStatusLabels).map(([value, label]) => ({ value, label }));
const methodOptions = Object.entries(paymentMethodLabels).map(([value, label]) => ({ value, label }));

export function PaymentsListPage() {
  const [statusFilter, setStatusFilter] = useState('');
  const [methodFilter, setMethodFilter] = useState('');

  const filters = {
    ...(statusFilter ? { status: statusFilter } : {}),
    ...(methodFilter ? { method: methodFilter } : {}),
  };

  const { data: payments = [], isLoading } = usePaymentsList(
    Object.keys(filters).length > 0 ? filters : undefined,
  );

  const hasFilters = !!statusFilter || !!methodFilter;

  const columns: DataGridColumn<PaymentDto>[] = [
    { header: 'رقم الدفعة', cell: (row) => <span className="font-mono">{row.paymentNumber}</span> },
    { header: 'رقم طلب الصرف', cell: (row) => <span className="font-mono">{row.disbursementRequestNumber}</span> },
    { header: 'رقم أمر الدفع', cell: (row) => <span className="font-mono">{row.paymentOrderNumber}</span> },
    { header: 'المبلغ', cell: (row) => <MoneyDisplay value={row.amount} /> },
    {
      header: 'طريقة الدفع',
      cell: (row) => <span className="text-sm">{paymentMethodLabels[row.paymentMethod as PaymentMethod] ?? row.paymentMethod}</span>,
    },
    {
      header: 'التاريخ',
      cell: (row) => <span className="text-sm">{row.paidAt ? new Date(row.paidAt).toLocaleDateString('ar-YE') : '—'}</span>,
    },
    { header: 'المرجع', cell: (row) => <span className="font-mono">{row.referenceNumber ?? '—'}</span> },
    {
      header: 'الحالة',
      cell: (row) => <PaymentsStatusBadge status={row.status} variant="payment" />,
    },
  ];

  return (
    <Page
      title="المدفوعات"
      loading={isLoading}
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={() => { setStatusFilter(''); setMethodFilter(''); }}>
          <FilterSelect
            value={statusFilter}
            onChange={setStatusFilter}
            options={statusOptions}
            placeholder="الحالة"
            label="الحالة"
          />
          <FilterSelect
            value={methodFilter}
            onChange={setMethodFilter}
            options={methodOptions}
            placeholder="طريقة الدفع"
            label="طريقة الدفع"
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={payments}
        loading={isLoading}
        emptyMessage="لا توجد مدفوعات"
        rowKey={(row) => row.id}
      />
    </Page>
  );
}
