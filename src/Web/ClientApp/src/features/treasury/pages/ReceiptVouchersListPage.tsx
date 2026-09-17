// Receipt vouchers list — filters (party / status / collection order).
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, FilterBar, FilterSelect, Badge, EmptyState, MoneyDisplay, Page } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { ReceiptVouchersClient, ReceiptVoucherStatus } from '../../../web-api-client';
import { formatDate } from '@/shared/utils/formatters';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { partiesClient } from '../../parties/shared/client';
import { voucherStatusLabels, voucherStatusBadgeVariant } from '../shared/types';
import type { PartyResponse } from '../../parties/shared/types';
import type { ReceiptVoucherDto } from '../shared/types';

const client = new ReceiptVouchersClient();

export default function ReceiptVouchersListPage() {
  const navigate = useNavigate();
  const [partyId, setPartyId] = useState('');
  const [status, setStatus] = useState('');

  const { data: parties = [] } = useQuery({
    queryKey: ['parties', 'active'],
    queryFn: () => partiesClient.list({ isActive: true }),
  });

  const { data: vouchers = [], isLoading, isError: isVouchersError, error: vouchersError, refetch: refetchVouchers } = useQuery({
    queryKey: ['receipt-vouchers', 'list', partyId, status],
    queryFn: () =>
      client.receiptVouchersAll(
        undefined,
        partyId ? Number(partyId) : undefined,
        status ? (status as ReceiptVoucherStatus) : undefined,
      ),
  });

  const columns: DataGridColumn<ReceiptVoucherDto>[] = useMemo(
    () => [
      {
        key: 'voucherNumber',
        header: 'رقم السند',
        render: (row) => <span className="font-medium tabular-nums">{row.voucherNumber}</span>,
      },
      { key: 'voucherDate', header: 'التاريخ', render: (row) => formatDate(row.voucherDate) },
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

  const hasFilters = !!partyId || !!status;
  const partyOptions: { value: string; label: string }[] = (parties as PartyResponse[]).map((p) => ({
    value: String(p.id),
    label: p.nameAr,
  }));

  return (
    <Page
      title="سندات القبض"
      actions={
        <Button onClick={() => navigate('/treasury/receipt-vouchers/create')}>
          <Plus size={16} />
          سند جديد
        </Button>
      }
      toolbar={
        <FilterBar
          hasFilters={hasFilters}
          onClear={() => {
            setPartyId('');
            setStatus('');
          }}
        >
          <FilterSelect
            label="الجهة"
            value={partyId}
            onChange={setPartyId}
            options={[{ value: '', label: 'الكل' }, ...partyOptions]}
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
        </FilterBar>
      }
      loading={isLoading}
    >
      {isVouchersError ? (
        <div className="p-4 text-center flex flex-col items-center gap-2">
          <p className="text-sm text-[var(--color-error)]">{getQueryErrorMessage(vouchersError)}</p>
          <Button variant="outline" size="sm" onClick={() => refetchVouchers()}>إعادة المحاولة</Button>
        </div>
      ) : vouchers.length === 0 ? (
        <EmptyState message="لم يتم العثور على سندات قبض مطابقة للفلاتر المحددة." />
      ) : (
        <DataGrid columns={columns} data={vouchers} rowKey={(row) => row.id ?? 0} />
      )}
    </Page>
  );
}
