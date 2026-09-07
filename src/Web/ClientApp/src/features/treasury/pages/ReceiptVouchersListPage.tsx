// Receipt vouchers list — US3: filters (party / period / status / method).
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, FilterBar, FilterDate, FilterSelect, Badge, EmptyState, Loading, MoneyDisplay, PageHeader } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { ReceiptVouchersClient, PaymentMethod, ReceiptVoucherStatus } from '../../../web-api-client';
import { partiesClient } from '../../parties/shared/client';
import { voucherStatusLabels, voucherStatusBadgeVariant } from '../shared/types';
import type { PartyResponse } from '../../parties/shared/types';
import type { ReceiptVoucherDto } from '../shared/types';

const client = new ReceiptVouchersClient();

export default function ReceiptVouchersListPage() {
  const navigate = useNavigate();
  const [partyId, setPartyId] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('');
  const [status, setStatus] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const { data: parties = [] } = useQuery({
    queryKey: ['parties', 'active'],
    queryFn: () => partiesClient.list({ isActive: true }),
  });

  const filters = useMemo(
    () => ({
      partyId: partyId ? Number(partyId) : undefined,
      paymentMethod: paymentMethod ? (paymentMethod as PaymentMethod) : undefined,
      status: status ? (status as ReceiptVoucherStatus) : undefined,
      fromDate: fromDate ? new Date(fromDate) : undefined,
      toDate: toDate ? new Date(toDate) : undefined,
    }),
    [partyId, paymentMethod, status, fromDate, toDate],
  );

  const { data: vouchers = [], isLoading } = useQuery({
    queryKey: ['receipt-vouchers', 'list', filters],
    queryFn: () =>
      client.receiptVouchersAll(
        filters.partyId,
        filters.paymentMethod,
        filters.status,
        filters.fromDate,
        filters.toDate,
        1,
        50,
      ),
  });

  const columns: DataGridColumn<ReceiptVoucherDto>[] = useMemo(
    () => [
      {
        key: 'voucherNumber',
        header: 'رقم السند',
        render: (row) => <span className="font-medium tabular-nums">{row.voucherNumber}</span>,
      },
      { key: 'voucherDate', header: 'التاريخ', render: (row) => (row.voucherDate ? String(row.voucherDate) : '') },
      { key: 'partyName', header: 'الجهة' },
      { key: 'paymentMethodName', header: 'طريقة الدفع' },
      {
        key: 'status',
        header: 'الحالة',
        render: (row) => (
          <Badge variant={voucherStatusBadgeVariant[row.status ?? 'Draft']}>
            {voucherStatusLabels[row.status ?? 'Draft']}
          </Badge>
        ),
      },
      {
        key: 'totalAmount',
        header: 'الإجمالي',
        render: (row) => <MoneyDisplay value={row.totalAmount ?? 0} />,
      },
      {
        key: 'depositSlipNumber',
        header: 'بطاقة الإيداع',
        render: (row) =>
          row.depositSlipId ? (
            <span className="tabular-nums text-[var(--color-primary)]">
              {row.depositSlipNumber ?? row.depositSlipId}
            </span>
          ) : (
            <span className="text-[var(--color-on-surface-variant)]">—</span>
          ),
      },
      {
        key: 'actions',
        header: '',
        render: (row) => (
          <Button
            variant="ghost"
            size="icon"
            aria-label={`عرض السند ${row.voucherNumber}`}
            onClick={() => navigate(`/treasury/receipt-vouchers/${row.id}`)}
          >
            <Eye size={16} />
          </Button>
        ),
      },
    ],
    [navigate],
  );

  const hasFilters = !!partyId || !!paymentMethod || !!status || !!fromDate || !!toDate;
  const partyOptions: { value: string; label: string }[] = (parties as PartyResponse[]).map((p) => ({
    value: String(p.id),
    label: p.nameAr,
  }));

  return (
    <div className="space-y-6">
      <PageHeader
        title="سندات القبض"
        actions={
          <Button onClick={() => navigate('/treasury/receipt-vouchers/create')}>
            <Plus size={16} />
            سند جديد
          </Button>
        }
      />

      <FilterBar
        hasFilters={hasFilters}
        onClear={() => {
          setPartyId('');
          setPaymentMethod('');
          setStatus('');
          setFromDate('');
          setToDate('');
        }}
      >
        <FilterSelect
          label="الجهة"
          value={partyId}
          onChange={setPartyId}
          options={[{ value: '', label: 'الكل' }, ...partyOptions]}
        />
        <FilterSelect
          label="طريقة الدفع"
          value={paymentMethod}
          onChange={setPaymentMethod}
          options={[
            { value: '', label: 'الكل' },
            { value: PaymentMethod.Cash, label: 'نقدي' },
            { value: PaymentMethod.Check, label: 'شيكات' },
          ]}
        />
        <FilterSelect
          label="الحالة"
          value={status}
          onChange={setStatus}
          options={[
            { value: '', label: 'الكل' },
            ...Object.entries(voucherStatusLabels).map(([value, label]) => ({ value, label })),
          ]}
        />
        <FilterDate label="من تاريخ" value={fromDate} onChange={setFromDate} />
        <FilterDate label="إلى تاريخ" value={toDate} onChange={setToDate} />
      </FilterBar>

      {isLoading ? (
        <Loading />
      ) : vouchers.length === 0 ? (
        <EmptyState message="لم يتم العثور على سندات قبض مطابقة للفلاتر المحددة." />
      ) : (
        <DataGrid columns={columns} data={vouchers} rowKey={(row) => row.id ?? 0} />
      )}
    </div>
  );
}
