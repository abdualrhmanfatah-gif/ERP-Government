import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Input, Select, DatePicker } from '@/components/ui';
import { partiesClient } from '../../parties/shared/client';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import {
  CreateReceiptVoucherCommand,
  CreateCheckDto,
  CreateReceiptVoucherLineDto,
  PaymentMethod,
} from '../../../web-api-client';
import { VoucherLinesEditor } from '@/components/TreasuryVoucherLinesEditor';
import { ChecksSection } from '@/components/TreasuryChecksSection';
import { paymentMethodLabels } from '../shared/types';
import type { LineFormRow, CheckFormRow } from '../shared/types';

interface ReceiptVoucherFormProps {
  onSubmit: (dto: CreateReceiptVoucherCommand) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  initialData?: {
    voucherDate: string;
    partyId: number;
    paymentMethod: number;
    receivedFrom?: string;
    notes?: string;
    lines: LineFormRow[];
    checks?: CheckFormRow[];
  };
  onEdit?: () => void;
}

export function ReceiptVoucherForm({
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  initialData,
  onEdit,
}: ReceiptVoucherFormProps) {
  const { data: parties = [] } = useQuery({
    queryKey: ['parties', 'active'],
    queryFn: () => partiesClient.list({ isActive: true }),
  });
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });

  const [voucherDate, setVoucherDate] = useState(initialData?.voucherDate ?? new Date().toISOString().slice(0, 10));
  const [partyId, setPartyId] = useState(initialData?.partyId ?? 0);
  const [paymentMethod, setPaymentMethod] = useState<string>(String(initialData?.paymentMethod ?? ''));
  const [receivedFrom, setReceivedFrom] = useState(initialData?.receivedFrom ?? '');
  const [notes, setNotes] = useState(initialData?.notes ?? '');
  const [lines, setLines] = useState<LineFormRow[]>(initialData?.lines ?? [{ revenueAccountId: 0, amount: 0 }]);
  const [checks, setChecks] = useState<CheckFormRow[]>(initialData?.checks ?? []);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const isCheck = paymentMethod === '2';
  const totalAmount = useMemo(() => lines.reduce((sum, l) => sum + (l.amount || 0), 0), [lines]);

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!voucherDate) e.voucherDate = 'تاريخ السند مطلوب';
    if (!partyId) e.partyId = 'الجهة مطلوبة';
    if (!paymentMethod) e.paymentMethod = 'طريقة الدفع مطلوبة';
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

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    onSubmit(
      new CreateReceiptVoucherCommand({
        voucherDate: new Date(voucherDate),
        partyId,
        paymentMethod: Number(paymentMethod) as PaymentMethod,
        receivedFrom: receivedFrom || undefined,
        notes: notes || undefined,
        lines: lines.map(
          (l) =>
            new CreateReceiptVoucherLineDto({
              revenueAccountId: l.revenueAccountId,
              amount: l.amount,
              description: l.description || undefined,
            }),
        ),
        checks: isCheck
          ? checks.map(
              (c) =>
                new CreateCheckDto({
                  bankName: c.bankName,
                  checkNumber: c.checkNumber,
                  checkDate: new Date(c.checkDate),
                  amount: c.amount,
                }),
            )
          : [],
      }),
    );
  }

  if (readOnly && initialData) {
    return (
      <div className="space-y-4">
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold">بيانات السند</h3>
            {onEdit && (
              <Button variant="outline" size="sm" onClick={onEdit}>
                تعديل
              </Button>
            )}
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-[var(--color-on-surface-variant)]">التاريخ:</span>{' '}
              <span>{initialData.voucherDate}</span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الجهة:</span>{' '}
              <span>{parties.find((p) => p.id === initialData.partyId)?.nameAr ?? `#${initialData.partyId}`}</span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">طريقة الدفع:</span>{' '}
              <span>{paymentMethodLabels[String(initialData.paymentMethod)] ?? initialData.paymentMethod}</span>
            </div>
            {initialData.receivedFrom && (
              <div>
                <span className="text-[var(--color-on-surface-variant)]">وارد من:</span>{' '}
                <span>{initialData.receivedFrom}</span>
              </div>
            )}
            {initialData.notes && (
              <div className="md:col-span-2">
                <span className="text-[var(--color-on-surface-variant)]">ملاحظات:</span>{' '}
                <span>{initialData.notes}</span>
              </div>
            )}
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الإجمالي:</span>{' '}
              <span className="font-semibold">{totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2 })}</span>
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-[var(--color-outline-variant)] p-6">
          <h3 className="mb-3 text-sm font-medium">بنود الإيراد</h3>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-xs text-[var(--color-on-surface-variant)]">
                <th className="py-2 text-start">حساب الإيراد</th>
                <th className="py-2 text-start">الوصف</th>
                <th className="py-2 text-end">المبلغ</th>
              </tr>
            </thead>
            <tbody>
              {lines.map((line, idx) => (
                <tr key={idx} className="border-t border-[var(--color-outline-variant)]">
                  <td className="py-2">{accounts.find((a) => a.id === line.revenueAccountId)?.name ?? `#${line.revenueAccountId}`}</td>
                  <td className="py-2">{line.description ?? '—'}</td>
                  <td className="py-2 text-end">{line.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {initialData.checks && initialData.checks.length > 0 && (
          <div className="rounded-lg border border-[var(--color-outline-variant)] p-6">
            <h3 className="mb-3 text-sm font-medium">الشيكات</h3>
            <table className="w-full text-sm">
              <thead>
                <tr className="text-xs text-[var(--color-on-surface-variant)]">
                  <th className="py-2 text-start">البنك</th>
                  <th className="py-2 text-start">رقم الشيك</th>
                  <th className="py-2 text-start">التاريخ</th>
                  <th className="py-2 text-end">المبلغ</th>
                </tr>
              </thead>
              <tbody>
                {initialData.checks.map((check, idx) => (
                  <tr key={idx} className="border-t border-[var(--color-outline-variant)]">
                    <td className="py-2">{check.bankName}</td>
                    <td className="py-2 tabular-nums">{check.checkNumber}</td>
                    <td className="py-2">{check.checkDate}</td>
                    <td className="py-2 text-end">{check.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6 rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="سند قبض">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div>
          <DatePicker
            label="تاريخ السند *"
            value={voucherDate}
            onChange={setVoucherDate}
            required
            error={errors.voucherDate}
          />
        </div>

        <div>
          <Select
            label="طريقة الدفع *"
            value={paymentMethod}
            onChange={(e) => {
              const method = e.target.value;
              setPaymentMethod(method);
              if (method === '2' && checks.length === 0) {
                setChecks([{ bankName: '', checkNumber: '', checkDate: '', amount: 0 }]);
              } else if (method === '1') {
                setChecks([]);
              }
            }}
            options={[
              { value: '', label: 'اختر طريقة الدفع...' },
              ...Object.entries(paymentMethodLabels).map(([value, label]) => ({ value, label })),
            ]}
          />
          {errors.paymentMethod && <p className="text-xs text-[var(--color-error)] mt-1">{errors.paymentMethod}</p>}
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
          <Input
            label="وارد من"
            type="text"
            value={receivedFrom}
            onChange={(e) => setReceivedFrom(e.target.value)}
            placeholder="اسم الدافع الفعلي إن اختلف عن الجهة"
          />
        </div>

        <div className="md:col-span-2">
          <Input
            label="ملاحظات"
            type="text"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
          />
        </div>
      </div>

      <VoucherLinesEditor
        rows={lines}
        accounts={accounts.map((a) => ({ id: a.id ?? 0, code: a.code ?? '', name: a.name ?? '' }))}
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
          الإجمالي
        </span>
        <span className="text-lg font-semibold tabular-nums text-[var(--color-on-surface)]">
          {totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
        </span>
      </div>

      {errors.submit && <p className="text-xs text-[var(--color-error)]">{errors.submit}</p>}

      <div className="flex justify-end gap-3">
        <Button type="button" variant="outline" onClick={onCancel}>
          إلغاء
        </Button>
        <Button type="submit" disabled={isPending} loading={isPending}>
          حفظ السند
        </Button>
      </div>
    </form>
  );
}
