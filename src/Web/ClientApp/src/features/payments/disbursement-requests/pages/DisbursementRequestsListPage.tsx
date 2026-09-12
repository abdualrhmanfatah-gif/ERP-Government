import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDisbursementRequestsList } from '../hooks/useDisbursementRequests';
import { disbursementRequestStatusOptions } from '../../shared/types';
import { PaymentsStatusBadge } from '@/components/PaymentsStatusBadge';
import { PaymentsDisbursementRequestSignatureProgress } from '@/components/PaymentsDisbursementRequestSignatureProgress';
import { Page, Button, FilterBar, MoneyDisplay, FilterSelect, DataGrid, type DataGridColumn } from '@/components/ui';
import type { DisbursementRequestListItem } from '../shared/types';
import { getFinalApprovedAmount } from '../shared/types';
import { Plus } from 'lucide-react';

export function DisbursementRequestsListPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState('');
  const [requesterFilter, setRequesterFilter] = useState('');

  const filters = {
    ...(statusFilter ? { status: Number(statusFilter) } : {}),
    ...(requesterFilter ? { requestedById: Number(requesterFilter) } : {}),
  };

  const { data: requests, isLoading } = useDisbursementRequestsList(
    Object.keys(filters).length > 0 ? filters : undefined,
  );

  const hasFilters = !!statusFilter || !!requesterFilter;

  const columns: DataGridColumn<DisbursementRequestListItem>[] = [
    {
      header: 'رقم الطلب',
      cell: (row) => (
        <Button
          variant="link"
          className="font-mono"
          onClick={() => navigate(`/payments/disbursement-requests/${row.id}`)}
        >
          {row.requestNumber}
        </Button>
      ),
    },
    { header: 'التاريخ', cell: (row) => <span className="text-sm">{row.requestDate}</span> },
    { header: 'مقدم الطلب', cell: (row) => <span className="text-sm">{row.requestedByName}</span> },
    { header: 'المستفيد', cell: (row) => <span className="text-sm">{row.beneficiaryName}</span> },
    {
      header: 'المبلغ المطلوب',
      cell: (row) => <MoneyDisplay value={row.requestedAmount} />,
    },
    { header: 'العملة', cell: (row) => <span className="text-sm">{row.currencyId}</span> },
    {
      header: 'المعتمد النهائي',
      cell: (row) => {
        const amount = getFinalApprovedAmount(row.approvals ?? []);
        return amount != null ? (
          <MoneyDisplay value={amount} />
        ) : (
          <span className="text-[var(--color-on-surface-variant)]">لم يعتمد بعد</span>
        );
      },
    },
    {
      header: 'تقدم التوقيعان',
      cell: (row) => <PaymentsDisbursementRequestSignatureProgress approvals={row.approvals ?? []} />,
    },
    {
      header: 'الحالة',
      cell: (row) => <PaymentsStatusBadge status={row.status} variant="request" />,
    },
    {
      header: 'الأمر الناتج',
      cell: (row) =>
        row.paymentOrderId ? (
          <Button
            variant="link"
            className="font-mono"
            onClick={() => navigate(`/payments/payment-orders/${row.paymentOrderId}`)}
          >
            {row.paymentOrderNumber ?? row.paymentOrderId}
          </Button>
        ) : (
          <span className="text-[var(--color-on-surface-variant)]">—</span>
        ),
    },
    {
      header: 'قيد الاستحقاق',
      cell: (row) =>
        row.accrualJournalEntryId ? (
          <Button
            variant="link"
            className="font-mono"
            onClick={() => navigate(`/accounting/journal-entries/${row.accrualJournalEntryId}`)}
          >
            {row.accrualEntryNumber ?? row.accrualJournalEntryId}
          </Button>
        ) : (
          <span className="text-[var(--color-on-surface-variant)]">—</span>
        ),
    },
  ];

  return (
    <Page
      title="طلبات الصرف"
      actions={
        <Button variant="primary" size="sm" onClick={() => navigate('/payments/disbursement-requests/create')}>
          <Plus size={16} className="ms-1" />
          طلب صرف جديد
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={() => { setStatusFilter(''); setRequesterFilter(''); }}>
          <FilterSelect
            value={statusFilter}
            onChange={setStatusFilter}
            options={disbursementRequestStatusOptions}
            placeholder="الحالة"
            label="الحالة"
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={requests ?? []}
        loading={isLoading}
        emptyMessage="لا توجد طلبات صرف"
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/payments/disbursement-requests/${row.id}`)}
      />
    </Page>
  );
}
