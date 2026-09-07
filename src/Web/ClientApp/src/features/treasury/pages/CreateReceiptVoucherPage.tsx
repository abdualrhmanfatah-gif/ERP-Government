// Create receipt voucher — US1: instant DSL number + server-computed total (FR-001/004).
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, Input, Select, Textarea, Page } from '@/components/ui';
import { ArrowRight } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { partiesClient } from '../../parties/shared/client';
import { AccountsClient } from '../../../web-api-client';
import {
  CreateReceiptVoucherCommand,
  CreateCheckDto,
  CreateReceiptVoucherLineDto,
} from '../../../web-api-client';
import { useCreateReceiptVoucher } from '../hooks/useReceiptVouchers';
import { VoucherLinesEditor } from '@/components/TreasuryVoucherLinesEditor';
import { ChecksSection } from '@/components/TreasuryChecksSection';
import { paymentMethodLabels } from '../shared/types';
import type { LineFormRow, CheckFormRow } from '../shared/types';

const accountsClient = new AccountsClient();

export default function CreateReceiptVoucherPage() {
  const navigate = useNavigate();
  const createMutation = useCreateReceiptVoucher();

  const { data: parties = [] } = useQuery({
    queryKey: ['parties', 'active'],
    queryFn: () => partiesClient.list({ isActive: true }),
  });
  const { data: accounts = [] } = useQuery({
    queryKey: ['accounts', 'postable'],
    queryFn: () => accountsClient.accountsAll(true, undefined, true, undefined),
  });

  const [voucherDate, setVoucherDate] = useState(new Date().toISOString().slice(0, 10));
  const [partyId, setPartyId] = useState(0);
  const [paymentMethod, setPaymentMethod] = useState<number>(0);
  const [receivedFrom, setReceivedFrom] = useState('');
  const [notes, setNotes] = useState('');
  const [lines, setLines] = useState<LineFormRow[]>([{ revenueAccountId: 0, amount: 0 }]);
  const [checks, setChecks] = useState<CheckFormRow[]>([]);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const isCheck = paymentMethod === 1;
  const totalAmount = useMemo(() => lines.reduce((sum, l) => sum + (l.amount || 0), 0), [lines]);

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!voucherDate) e.voucherDate = 'تاريخ السند مطلوب';
    if (!partyId) e.partyId = 'الجهة مطلوبة';
    if (lines.length === 0) e.lines = 'بند إيراد واحد على الأقل مطلوب';
    if (lines.some((l) => !l.revenueAccountId)) e.lines = 'يجب اختيار حساب الإيراد لكل بند';
    if (lines.some((l) => l.amount <= 0)) e.lines = 'مبلغ البند يجب أن يكون أكبر من صفر';
    if (isCheck) {
      if (checks.length === 0) e.checks = 'شيك واحد على الأقل مطلوب لسندات الشيكات';
      if (checks.some((c) => !c.bankName || !c.checkNumber || !c.checkDate)) e.checks = 'جميع حقول الشيك مطلوبة';
      if (checks.some((c) => c.amount <= 0)) e.checks = 'مبلغ الشيك يجب أن يكون أكبر من صفر';
      const checksTotal = checks.reduce((s, c) => s + (c.amount || 0), 0);
      if (checksTotal > totalAmount) e.checks = 'مجموع الشيكات يتجاوز إجمالي السند';
    }
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    try {
      const dto = await createMutation.mutateAsync(
        new CreateReceiptVoucherCommand({
          voucherDate: new Date(voucherDate),
          partyId,
          paymentMethod: paymentMethod as never,
          receivedFrom: receivedFrom || undefined,
          notes: notes || undefined,
          lines: lines.map((l) => new CreateReceiptVoucherLineDto({
            revenueAccountId: l.revenueAccountId,
            amount: l.amount,
            description: l.description || undefined,
          })),
          checks: isCheck
            ? checks.map(
                (c) => new CreateCheckDto({ bankName: c.bankName, checkNumber: c.checkNumber, checkDate: new Date(c.checkDate), amount: c.amount }),
              )
            : [],
        }),
      );
      notify({ type: 'success', title: `تم إنشاء السند برقم ${dto.voucherNumber}` });
      navigate(`/treasury/receipt-vouchers/${dto.id}`);
    } catch (err: unknown) {
      const problem = err as { detail?: string; title?: string };
      setErrors({ submit: problem.detail ?? problem.title ?? 'حدث خطأ أثناء الحفظ' });
    }
  }

  return (
    <Page title="سند قبض جديد">
      <Button variant="ghost" size="icon" onClick={() => navigate('/treasury/receipt-vouchers')} aria-label="العودة" className="mb-4">
        <ArrowRight size={18} />
      </Button>

      <form onSubmit={handleSubmit} className="space-y-6 rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <Input
              label="تاريخ السند *"
              type="date"
              value={voucherDate}
              onChange={(e) => setVoucherDate(e.target.value)}
              required
            />
            {errors.voucherDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.voucherDate}</p>}
          </div>

          <div>
            <Select
              label="الجهة *"
              value={String(partyId)}
              onChange={(e) => setPartyId(Number(e.target.value))}
              options={[
                { value: '0', label: 'اختر الجهة...' },
                ...parties.map((p) => ({ value: String(p.id), label: p.nameAr })),
              ]}
            />
            {errors.partyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.partyId}</p>}
          </div>

          <div>
            <Select
              label="طريقة الدفع *"
              value={String(paymentMethod)}
              onChange={(e) => {
                const method = Number(e.target.value);
                setPaymentMethod(method);
                if (method === 1 && checks.length === 0) {
                  setChecks([{ bankName: '', checkNumber: '', checkDate: '', amount: 0 }]);
                }
              }}
              options={Object.entries(paymentMethodLabels).map(([value, label]) => ({ value, label }))}
            />
          </div>

          <div>
            <Input
              label="وارد من"
              type="text"
              value={receivedFrom}
              onChange={(e) => setReceivedFrom(e.target.value)}
              placeholder="اسم الدافع الفعلي إن اختلف عن الجهة"
            />
          </div>

          <div className="md:col-span-2">
            <Textarea
              label="ملاحظات"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
            />
          </div>
        </div>

        <VoucherLinesEditor
          rows={lines}
          accounts={accounts.map((a) => ({ id: a.id ?? 0, name: a.name ?? a.code ?? '' }))}
          onChange={setLines}
          error={errors.lines}
        />

        <ChecksSection
          visible={isCheck}
          rows={checks}
          onChange={setChecks}
          error={errors.checks}
        />

        <div className="flex items-center justify-between rounded bg-[var(--color-surface-container)] px-4 py-3">
          <span className="text-sm text-[var(--color-on-surface-variant)]">
            الإجمالي (يُحسب خادمياً عند الحفظ)
          </span>
          <span className="text-lg font-semibold tabular-nums text-[var(--color-on-surface)]">
            {totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
          </span>
        </div>

        {errors.submit && <p className="text-xs text-[var(--color-error)]">{errors.submit}</p>}

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate('/treasury/receipt-vouchers')}>
            إلغاء
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'جارٍ الحفظ...' : 'حفظ السند'}
          </Button>
        </div>
      </form>
    </Page>
  );
}
