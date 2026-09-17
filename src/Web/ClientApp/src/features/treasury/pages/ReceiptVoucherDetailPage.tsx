import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Button, Badge, Card, Dialog, MoneyDisplay, Textarea, Page, Alert } from '@/components/ui';
import { Printer } from 'lucide-react';
import { useUserDetail } from '../../security/users/hooks/useUserDetail';
import { notify } from '@/features/notifications/notify';
import { MetaItem } from '@/components/MetaItem';
import {
  useReceiptVoucher,
  useSubmitReceiptVoucher,
  useApproveReceiptVoucher,
  useCancelReceiptVoucher,
  useUpdateReceiptVoucher,
} from '../hooks/useReceiptVouchers';
import { ReceiptVoucherForm } from '../components/ReceiptVoucherForm';
import { voucherStatusLabels, voucherStatusBadgeVariant } from '../shared/types';
import { ReceiptVoucherStatus, CreateReceiptVoucherCommand, UpdateReceiptVoucherCommand } from '../../../web-api-client';
import { formatDate, formatDateTime } from '@/shared/utils/formatters';

function ReviewerName({ userId }: { userId?: number }) {
  const enabled = !!userId;
  const { data: user } = useUserDetail(enabled ? userId! : 0);
  if (!userId) return <span className="text-[var(--color-on-surface-variant)]">—</span>;
  return <span>{user?.login ?? `مستخدم #${userId}`}</span>;
}

function SummaryField({ label, children, className }: { label: string; children: React.ReactNode; className?: string }) {
  return (
    <div className={className}>
      <dt className="text-xs text-[var(--color-on-surface-variant)] mb-1">{label}</dt>
      <dd className="text-sm font-medium text-[var(--color-on-surface)]">{children}</dd>
    </div>
  );
}

export function ReceiptVoucherDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const voucherId = Number(id);
  const { data: voucher, isLoading } = useReceiptVoucher(voucherId);

  const submitMutation = useSubmitReceiptVoucher();
  const approveMutation = useApproveReceiptVoucher();
  const cancelMutation = useCancelReceiptVoucher();
  const updateMutation = useUpdateReceiptVoucher();

  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [actionError, setActionError] = useState('');
  const [isEditing, setIsEditing] = useState(false);

  if (isLoading) return <Page title="سند القبض" loading>{undefined}</Page>;
  if (!voucher || !voucher.id)
    return (
      <Page title="خطأ" error="السند غير موجود." onRetry={() => navigate('/treasury/receipt-vouchers')}>
        <Button variant="outline" onClick={() => navigate('/treasury/receipt-vouchers')}>العودة للقائمة</Button>
      </Page>
    );

  const rowVersion = voucher.rowVersion ?? '';
  const status = voucher.status ?? ReceiptVoucherStatus.Draft;
  const lines = voucher.lines ?? [];
  const checks = voucher.checks ?? [];
  const isDraft = status === ReceiptVoucherStatus.Draft;
  const linesTotal = lines.reduce((sum, l) => sum + (l.amount ?? 0), 0);

  const paymentMethodLabel = voucher.paymentMethodName === 'Cash' || Number(voucher.paymentMethod) === 1
    ? 'نقدي'
    : voucher.paymentMethodName === 'Check' || Number(voucher.paymentMethod) === 2
    ? 'شيكات'
    : (voucher.paymentMethodName || '—');

  async function submit() {
    setActionError('');
    try {
      await submitMutation.mutateAsync({ id: voucherId, rowVersion });
      notify({ type: 'success', title: 'تم إرسال السند للمراجعة' });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'فشل الإرسال';
      setActionError(message);
      notify({ type: 'error', title: 'فشل الإرسال', message });
    }
  }

  async function approve() {
    setActionError('');
    try {
      await approveMutation.mutateAsync({ id: voucherId, rowVersion });
      notify({ type: 'success', title: 'تم اعتماد السند' });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'فشل الاعتماد';
      setActionError(message);
      notify({ type: 'error', title: 'فشل الاعتماد', message });
    }
  }

  async function cancel() {
    setActionError('');
    try {
      await cancelMutation.mutateAsync({ id: voucherId, rowVersion, reason: cancelReason.trim() });
      notify({ type: 'success', title: 'تم إلغاء السند' });
      setCancelOpen(false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'فشل الإلغاء';
      setActionError(message);
      notify({ type: 'error', title: 'فشل الإلغاء', message });
    }
  }

  async function handleUpdate(dto: CreateReceiptVoucherCommand) {
    try {
      await updateMutation.mutateAsync(new UpdateReceiptVoucherCommand({
        id: voucherId,
        voucherDate: dto.voucherDate,
        partyId: dto.partyId,
        paymentMethod: dto.paymentMethod,
        receivedFrom: dto.receivedFrom,
        notes: dto.notes,
        lines: dto.lines,
        checks: dto.checks,
        rowVersion: voucher?.rowVersion ?? '',
      }));
      notify({ type: 'success', title: 'تم حفظ التعديلات' });
      setIsEditing(false);
    } catch (err) {
      let msg = 'حدث خطأ أثناء الحفظ';
      if (err && typeof err === 'object' && 'response' in err) {
        const ex = err as { response: string; message: string };
        try { const b = JSON.parse(ex.response); msg = Array.isArray(b) ? b.join('\n') : (b.detail ?? b.title ?? ex.message); } catch { msg = ex.message; }
      }
      notify({ type: 'error', title: msg });
    }
  }

  const headerActions = (
    <div className="flex items-center gap-2">
      <Button variant="outline" size="sm" onClick={() => window.print()} className="gap-1.5">
        <Printer size={15} />
        طباعة السند
      </Button>
      {isDraft && !isEditing && (
        <Button variant="outline" onClick={() => setIsEditing(true)}>
          تعديل
        </Button>
      )}
      {isDraft && !isEditing && (
        <Button disabled={submitMutation.isPending} loading={submitMutation.isPending} onClick={submit}>
          إرسال للمراجعة
        </Button>
      )}
      {isDraft && !isEditing && (
        <Button disabled={approveMutation.isPending} loading={approveMutation.isPending} onClick={approve}>
          اعتماد
        </Button>
      )}
      {isDraft && !isEditing && (
        <Button variant="destructive" onClick={() => setCancelOpen(true)}>
          إلغاء السند
        </Button>
      )}
      {isEditing && (
        <Button variant="outline" onClick={() => setIsEditing(false)}>
          إلغاء التعديل
        </Button>
      )}
    </div>
  );

  if (isEditing) {
    return (
      <Page
        title={`تعديل سند القبض ${voucher.voucherNumber}`}
        onBack={() => setIsEditing(false)}
        actions={headerActions}
      >
        <ReceiptVoucherForm
          onSubmit={handleUpdate}
          onCancel={() => setIsEditing(false)}
          isPending={updateMutation.isPending}
          initialData={{
            collectionOrderId: voucher.collectionOrderId ?? 0,
            voucherDate: String(voucher.voucherDate ?? ''),
            partyId: voucher.partyId ?? 0,
            paymentMethod: Number(voucher.paymentMethod ?? 0),
            receivedFrom: voucher.receivedFrom ?? '',
            notes: voucher.notes ?? '',
            lines: (voucher.lines ?? []).map((l) => ({
              revenueAccountId: l.revenueAccountId ?? 0,
              amount: l.amount ?? 0,
              description: l.description ?? '',
            })),
            checks: (voucher.checks ?? []).map((c) => ({
              bankName: c.bankName ?? '',
              checkNumber: c.checkNumber ?? '',
              checkDate: String(c.checkDate ?? ''),
              amount: c.amount ?? 0,
            })),
          }}
        />
      </Page>
    );
  }

  return (
    <Page
      title={`سند قبض ${voucher.voucherNumber}`}
      onBack={() => navigate('/treasury/receipt-vouchers')}
      actions={headerActions}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-[var(--radius-lg)] bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <Badge variant={voucherStatusBadgeVariant[status]}>
            {voucherStatusLabels[status]}
          </Badge>
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="التاريخ" value={formatDate(voucher.voucherDate)} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="طريقة الدفع" value={paymentMethodLabel} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="الجهة" value={voucher.partyName} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="الإجمالي" value={<MoneyDisplay value={voucher.totalAmount ?? 0} />} />
        </div>
      }
    >
      {actionError && (
        <Alert variant="error" className="mb-4">
          {actionError}
        </Alert>
      )}

      <div className="space-y-6">
        {/* بيانات السند والطرف المتعامل */}
        <Card className="bg-[var(--color-surface-container-lowest)]">
          <h2 className="text-[var(--typography-label-md-size)] font-semibold mb-4 text-[var(--color-on-surface)]">
            بيانات السند والطرف المتعامل
          </h2>
          <dl className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <SummaryField label="الجهة / المورد">
              {voucher.partyName ?? '—'}
            </SummaryField>

            <SummaryField label="تاريخ السند">
              {formatDate(voucher.voucherDate)}
            </SummaryField>

            <SummaryField label="طريقة الدفع">
              {paymentMethodLabel}
            </SummaryField>

            {voucher.receivedFrom && (
              <SummaryField label="وارد من (المسلم)">
                {voucher.receivedFrom}
              </SummaryField>
            )}

            {voucher.depositSlipId && (
              <SummaryField label="بطاقة الإيداع">
                <Link
                  to={`/treasury/deposit-slips/${voucher.depositSlipId}`}
                  className="text-[var(--color-primary)] hover:underline tabular-nums font-semibold"
                >
                  {voucher.depositSlipNumber ?? `#${voucher.depositSlipId}`}
                </Link>
              </SummaryField>
            )}

            <SummaryField label="إجمالي المبلغ">
              <MoneyDisplay value={voucher.totalAmount ?? 0} />
            </SummaryField>

            {voucher.notes && (
              <SummaryField label="ملاحظات" className="md:col-span-3">
                {voucher.notes}
              </SummaryField>
            )}
          </dl>

          {/* معلومات الاعتماد والمراجعة */}
          <div className="mt-6 pt-4 border-t border-[var(--color-outline-variant)]">
            <dl className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <SummaryField label="قدّمه (المحرر)">
                {voucher.submittedById ? <>#{voucher.submittedById} · {formatDateTime(voucher.submittedAt)}</> : '—'}
              </SummaryField>
              <SummaryField label="راجعه (المراجع)">
                <ReviewerName userId={voucher.reviewedById ?? undefined} />
                {voucher.reviewedAt && (
                  <span className="text-[var(--color-on-surface-variant)] text-xs ms-1">
                    · {formatDateTime(voucher.reviewedAt)}
                  </span>
                )}
              </SummaryField>
            </dl>
          </div>

          {voucher.cancellationReason && (
            <div className="mt-4 pt-4 border-t border-[var(--color-error)]">
              <SummaryField label="سبب الإلغاء">
                <span className="text-[var(--color-error)] font-medium">{voucher.cancellationReason}</span>
              </SummaryField>
            </div>
          )}
        </Card>

        {/* بنود الإيراد */}
        <Card className="bg-[var(--color-surface-container-lowest)]">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-[var(--typography-label-md-size)] font-semibold text-[var(--color-on-surface)]">
              بنود الإيراد ({lines.length})
            </h2>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm" aria-label="بنود الإيراد">
              <thead>
                <tr className="border-b border-[var(--color-outline-variant)] text-xs text-[var(--color-on-surface-variant)]">
                  <th className="pb-2 text-start font-medium">حساب الإيراد</th>
                  <th className="pb-2 text-start font-medium">الوصف</th>
                  <th className="pb-2 text-end font-medium">المبلغ</th>
                </tr>
              </thead>
              <tbody>
                {lines.map((line) => (
                  <tr key={line.id} className="border-b border-[var(--color-outline-variant)] last:border-b-0">
                    <td className="py-2.5 font-mono text-xs font-semibold">{line.revenueAccountId}</td>
                    <td className="py-2.5 text-[var(--color-on-surface-variant)]">{line.description ?? '—'}</td>
                    <td className="py-2.5 text-end tabular-nums"><MoneyDisplay value={line.amount ?? 0} /></td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr className="border-t border-[var(--color-outline-variant)] font-semibold">
                  <td className="pt-3 text-start text-xs text-[var(--color-on-surface-variant)]">الإجمالي</td>
                  <td></td>
                  <td className="pt-3 text-end tabular-nums">
                    <MoneyDisplay value={linesTotal} />
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </Card>

        {/* الشيكات */}
        {checks.length > 0 && (
          <Card className="bg-[var(--color-surface-container-lowest)]">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-[var(--typography-label-md-size)] font-semibold text-[var(--color-on-surface)]">
                الشيكات ({checks.length})
              </h2>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm" aria-label="الشيكات">
                <thead>
                  <tr className="border-b border-[var(--color-outline-variant)] text-xs text-[var(--color-on-surface-variant)]">
                    <th className="pb-2 text-start font-medium">البنك</th>
                    <th className="pb-2 text-start font-medium">رقم الشيك</th>
                    <th className="pb-2 text-start font-medium">التاريخ</th>
                    <th className="pb-2 text-end font-medium">المبلغ</th>
                  </tr>
                </thead>
                <tbody>
                  {checks.map((check) => (
                    <tr key={check.id} className="border-b border-[var(--color-outline-variant)] last:border-b-0">
                      <td className="py-2.5">{check.bankName}</td>
                      <td className="py-2.5 tabular-nums font-mono text-xs">{check.checkNumber}</td>
                      <td className="py-2.5 text-xs text-[var(--color-on-surface-variant)]">{formatDate(check.checkDate)}</td>
                      <td className="py-2.5 text-end tabular-nums"><MoneyDisplay value={check.amount ?? 0} /></td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="border-t border-[var(--color-outline-variant)] font-semibold">
                    <td className="pt-3 text-start text-xs text-[var(--color-on-surface-variant)]">الإجمالي</td>
                    <td></td>
                    <td></td>
                    <td className="pt-3 text-end tabular-nums">
                      <MoneyDisplay value={checks.reduce((s, c) => s + (c.amount ?? 0), 0)} />
                    </td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </Card>
        )}
      </div>

      <Dialog open={cancelOpen} onClose={() => setCancelOpen(false)} title="إلغاء السند — السبب مطلوب">
        <div className="space-y-4">
          <Textarea
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            rows={3}
            placeholder="اذكر سبب الإلغاء..."
            label="سبب الإلغاء"
          />
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={() => setCancelOpen(false)}>تراجع</Button>
            <Button variant="destructive" disabled={!cancelReason.trim() || cancelMutation.isPending} loading={cancelMutation.isPending} onClick={cancel}>
              تأكيد الإلغاء
            </Button>
          </div>
        </div>
      </Dialog>
    </Page>
  );
}
