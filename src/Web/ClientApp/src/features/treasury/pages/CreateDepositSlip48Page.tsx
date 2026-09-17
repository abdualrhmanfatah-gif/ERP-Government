import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, DatePicker, Page, EmptyState, MoneyDisplay } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { notify } from '@/features/notifications/notify';
import { ChecksClient, DepositSlipsClient, CheckStatus, CreateDepositSlip48Command } from '@/web-api-client';
import { formatDate } from '@/shared/utils/formatters';

const checksClient = new ChecksClient();
const depositClient = new DepositSlipsClient();

export default function CreateDepositSlip48Page() {
  const navigate = useNavigate();
  const [slipDate, setSlipDate] = useState(new Date().toISOString().slice(0, 10));
  const [selectedIds, setSelectedIds] = useState<number[]>([]);

  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);

  const { data: checks = [], isLoading } = useQuery({
    queryKey: ['checks', 'received'],
    queryFn: () => checksClient.checksAll(firstDay, today, CheckStatus.Received),
  });

  const totalSelected = useMemo(
    () => checks.filter((c) => selectedIds.includes(c.id ?? 0)).reduce((s, c) => s + (c.amount ?? 0), 0),
    [checks, selectedIds],
  );

  function toggle(id: number) {
    setSelectedIds((prev) => (prev.includes(id) ? prev.filter((i) => i !== id) : [...prev, id]));
  }

  function selectAll() {
    setSelectedIds(checks.map((c) => c.id ?? 0));
  }

  async function handleSubmit() {
    if (selectedIds.length === 0) {
      notify({ type: 'error', title: 'يجب اختيار شيك واحد على الأقل' });
      return;
    }
    try {
      const result = await depositClient.slip48(
        new CreateDepositSlip48Command({
          slipDate: new Date(slipDate),
          checkIds: selectedIds,
        }),
      );
      notify({ type: 'success', title: `تم إنشاء حافظة الشيكات برقم ${result.value?.slipNumber ?? ''}` });
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

  const columns: DataGridColumn<typeof checks[number]>[] = useMemo(
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
            aria-label={`تحديد الشيك ${row.checkNumber}`}
          />
        ),
      },
      { key: 'checkNumber', header: 'رقم الشيك', render: (row) => <span className="font-medium tabular-nums">{row.checkNumber}</span> },
      { key: 'bankName', header: 'البنك' },
      {
        key: 'checkDate',
        header: 'تاريخ الشيك',
        render: (row) => formatDate(row.checkDate),
      },
      {
        key: 'amount',
        header: 'المبلغ',
        render: (row) => <MoneyDisplay value={row.amount ?? 0} />,
      },
    ],
    [selectedIds],
  );

  return (
    <Page
      title="إنشاء حافظة إرسال الشيكات (48)"
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
        ) : checks.length === 0 ? (
          <EmptyState message="لا توجد شيكات مستلمة متاحة للإرسال للتحصيل" />
        ) : (
          <>
            <DataGrid columns={columns} data={checks} rowKey={(row) => row.id ?? 0} />
            <div className="flex items-center justify-between rounded bg-[var(--color-surface-container)] px-4 py-3">
              <span className="text-sm text-[var(--color-on-surface-variant)]">
                المحدد: {selectedIds.length} شيك — الإجمالي
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
