import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, DatePicker, Page, EmptyState, MoneyDisplay } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { notify } from '@/features/notifications/notify';
import { ReceiptVouchersClient, DepositSlipsClient, PaymentMethod, ReceiptVoucherStatus, CreateDepositSlip47Command } from '@/web-api-client';
import { formatDate } from '@/shared/utils/formatters';

const receiptClient = new ReceiptVouchersClient();
const depositClient = new DepositSlipsClient();

export default function CreateDepositSlip47Page() {
  const navigate = useNavigate();
  const [slipDate, setSlipDate] = useState(new Date().toISOString().slice(0, 10));
  const [selectedIds, setSelectedIds] = useState<number[]>([]);

  const { data: allVouchers = [], isLoading } = useQuery({
    queryKey: ['receipt-vouchers', 'cash-approved'],
    queryFn: () => receiptClient.receiptVouchersAll(undefined, undefined, ReceiptVoucherStatus.Approved),
  });

  const cashVouchers = useMemo(
    () => allVouchers.filter((v) => v.paymentMethod === PaymentMethod.Cash && !v.depositSlipId),
    [allVouchers],
  );

  const totalSelected = useMemo(
    () => cashVouchers.filter((v) => selectedIds.includes(v.id ?? 0)).reduce((s, v) => s + (v.totalAmount ?? 0), 0),
    [cashVouchers, selectedIds],
  );

  function toggle(id: number) {
    setSelectedIds((prev) => (prev.includes(id) ? prev.filter((i) => i !== id) : [...prev, id]));
  }

  function selectAll() {
    setSelectedIds(cashVouchers.map((v) => v.id ?? 0));
  }

  async function handleSubmit() {
    if (selectedIds.length === 0) {
      notify({ type: 'error', title: 'يجب اختيار سند قبض واحد على الأقل' });
      return;
    }
    try {
      const result = await depositClient.slip47(
        new CreateDepositSlip47Command({
          slipDate: new Date(slipDate),
          receiptVoucherIds: selectedIds,
        }),
      );
      notify({ type: 'success', title: `تم إنشاء حافظة التوريد برقم ${result.value?.slipNumber ?? ''}` });
      navigate('/treasury/deposit-slips');
    } catch (err: unknown) {
      let message = 'حدث خطأ أثناء الحفظ';
      if (err && typeof err === 'object' && 'response' in err) {
        const ex = err as { response: string; message: string; status: number };
        try {
          const body = JSON.parse(ex.response);
          message = Array.isArray(body) ? body.join('\n') : (body.detail ?? body.title ?? ex.message);
        } catch {
          message = ex.message || `HTTP ${ex.status}`;
        }
      }
      notify({ type: 'error', title: message });
    }
  }

  const columns: DataGridColumn<typeof cashVouchers[number]>[] = useMemo(
    () => [
      {
        key: 'select',
        header: '',
        render: (row) => (
          <input
            type="checkbox"
            checked={selectedIds.includes(row.id ?? 0)}
            onChange={() => toggle(row.id ?? 0)}
            className="h-4 w-4"
            aria-label={`تحديد السند ${row.voucherNumber}`}
          />
        ),
      },
      {
        key: 'voucherNumber',
        header: 'رقم السند',
        render: (row) => <span className="font-medium tabular-nums">{row.voucherNumber}</span>,
      },
      {
        key: 'voucherDate',
        header: 'التاريخ',
        render: (row) => formatDate(row.voucherDate),
      },
      { key: 'partyName', header: 'الجهة' },
      {
        key: 'totalAmount',
        header: 'المبلغ',
        render: (row) => <MoneyDisplay value={row.totalAmount ?? 0} />,
      },
    ],
    [selectedIds],
  );

  return (
    <Page
      title="إنشاء حافظة توريد النقد (47)"
      onBack={() => navigate('/treasury/deposit-slips')}
    >
      <div className="space-y-6">
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <div className="flex items-center justify-between mb-4">
            <DatePicker
              label="تاريخ البطاقة *"
              value={slipDate}
              onChange={setSlipDate}
              required
            />
            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={selectAll}>
                تحديد الكل
              </Button>
              <Button variant="outline" size="sm" onClick={() => setSelectedIds([])}>
                إلغاء التحديد
              </Button>
            </div>
          </div>
        </div>

        {isLoading ? (
          <div className="p-4 text-center text-sm text-[var(--color-on-surface-variant)]">جاري التحميل...</div>
        ) : cashVouchers.length === 0 ? (
          <EmptyState message="لا توجد سندات قبض نقدية معتمدة متاحة للتوريد" />
        ) : (
          <>
            <DataGrid columns={columns} data={cashVouchers} rowKey={(row) => row.id ?? 0} />
            <div className="flex items-center justify-between rounded bg-[var(--color-surface-container)] px-4 py-3">
              <span className="text-sm text-[var(--color-on-surface-variant)]">
                المحدد: {selectedIds.length} سند — الإجمالي
              </span>
              <MoneyDisplay value={totalSelected} />
            </div>
          </>
        )}

        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={() => navigate('/treasury/deposit-slips')}>
            إلغاء
          </Button>
          <Button onClick={handleSubmit} disabled={selectedIds.length === 0}>
            إنشاء الحافظة
          </Button>
        </div>
      </div>
    </Page>
  );
}
