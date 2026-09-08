// Receipt voucher detail — US2 lifecycle: submit / approve / cancel (reason required).
// Reviewer display name resolved from users lookup (SC-002); contract DTO stays literal.
import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Button, Badge, Card, Dialog, MoneyDisplay, Textarea, Page } from '@/components/ui';
import { ArrowRight } from 'lucide-react';
import { useUserDetail } from '../../security/users/hooks/useUserDetail';
import { notify } from '@/features/notifications/notify';
import {
  useReceiptVoucher,
  useSubmitReceiptVoucher,
  useApproveReceiptVoucher,
  useCancelReceiptVoucher,
} from '../hooks/useReceiptVouchers';
import { voucherStatusLabels, voucherStatusBadgeVariant } from '../shared/types';
import { ReceiptVoucherStatus } from '../../../web-api-client';

function ReviewerName({ userId }: { userId?: number }) {
  const enabled = !!userId;
  const { data: user } = useUserDetail(enabled ? userId! : 0);
  if (!userId) return <span className="text-[var(--color-on-surface-variant)]">—</span>;
  return <span>{user?.login ?? `مستخدم #${userId}`}</span>;
}

export default function ReceiptVoucherDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const voucherId = Number(id);
  const { data: voucher, isLoading } = useReceiptVoucher(voucherId);

  const submitMutation = useSubmitReceiptVoucher();
  const approveMutation = useApproveReceiptVoucher();
  const cancelMutation = useCancelReceiptVoucher();

  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [actionError, setActionError] = useState('');

  if (isLoading) return <Page title="">{undefined}</Page>;
  if (!voucher || !voucher.id)
    return (
      <Page title="" error="السند غير موجود." onRetry={() => navigate('/treasury/receipt-vouchers')}>
        <Button variant="outline" onClick={() => navigate('/treasury/receipt-vouchers')}>العودة للقائمة</Button>
      </Page>
    );

  const rowVersion = voucher.rowVersion ?? '';
  const status = voucher.status ?? ReceiptVoucherStatus.Draft;
  const lines = voucher.lines ?? [];
  const checks = voucher.checks ?? [];

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
      notify({ type: 'error', title: 'فشل الإلغاء', message });
    }
  }

  const canSubmit = status === ReceiptVoucherStatus.Draft && lines.length > 0;
  const canApprove = status === ReceiptVoucherStatus.PendingReview;
  const canCancel = status === ReceiptVoucherStatus.Draft || status === ReceiptVoucherStatus.PendingReview;

  return (
    <Page
      title={`سند القبض ${voucher.voucherNumber}`}
      actions={
        <Badge variant={voucherStatusBadgeVariant[status]}>{voucherStatusLabels[status]}</Badge>
      }
    >
      <Button variant="ghost" size="icon" onClick={() => navigate('/treasury/receipt-vouchers')} aria-label="العودة" className="mb-4">
        <ArrowRight size={18} />
      </Button>

      {actionError && (
        <p className="rounded bg-[var(--color-error-container)] px-4 py-2 text-xs text-[var(--color-on-error-container)]">
          {actionError}
        </p>
      )}

      <Card className="grid grid-cols-1 md:grid-cols-2 gap-6 bg-[var(--color-surface-container-lowest)]">
        <div><span className="text-xs text-[var(--color-on-surface-variant)]">التاريخ:</span> {voucher.voucherDate ? String(voucher.voucherDate) : ''}</div>
        <div><span className="text-xs text-[var(--color-on-surface-variant)]">الجهة:</span> {voucher.partyName}</div>
        <div><span className="text-xs text-[var(--color-on-surface-variant)]">طريقة الدفع:</span> {voucher.paymentMethodName}</div>
        {voucher.receivedFrom && (
          <div><span className="text-xs text-[var(--color-on-surface-variant)]">وارد من:</span> {voucher.receivedFrom}</div>
        )}
        {voucher.notes && (
          <div className="md:col-span-2"><span className="text-xs text-[var(--color-on-surface-variant)]">ملاحظات:</span> {voucher.notes}</div>
        )}
        <div>
          <span className="text-xs text-[var(--color-on-surface-variant)]">الإجمالي:</span>{' '}
          <MoneyDisplay value={voucher.totalAmount ?? 0} />
        </div>
        {voucher.depositSlipId ? (
          <div>
            <span className="text-xs text-[var(--color-on-surface-variant)]">بطاقة الإيداع:</span>{' '}
            <Link
              to={`/treasury/deposit-slips/${voucher.depositSlipId}`}
              className="text-[var(--color-primary)] hover:underline tabular-nums"
            >
              {voucher.depositSlipNumber ?? `#${voucher.depositSlipId}`}
            </Link>
          </div>
        ) : null}
        <div>
          <span className="text-xs text-[var(--color-on-surface-variant)]">قدّمه:</span>{' '}
          {voucher.submittedById ? <>#{voucher.submittedById} · {voucher.submittedAt ? new Date(voucher.submittedAt).toLocaleString('ar') : ''}</> : '—'}
        </div>
        <div>
          <span className="text-xs text-[var(--color-on-surface-variant)]">راجعه:</span>{' '}
          <ReviewerName userId={voucher.reviewedById ?? undefined} /> ·{' '}
          {voucher.reviewedAt ? new Date(voucher.reviewedAt).toLocaleString('ar') : '—'}
        </div>
        {voucher.cancellationReason && (
          <div className="md:col-span-2">
            <span className="text-xs text-[var(--color-on-surface-variant)]">سبب الإلغاء:</span> {voucher.cancellationReason}
          </div>
        )}
      </Card>

      <section aria-label="بنود الإيراد" className="rounded-lg border border-[var(--color-outline-variant)] p-6">
        <h2 className="mb-3 text-sm font-medium text-[var(--color-on-surface)]">بنود الإيراد</h2>
        <table className="w-full text-sm">
          <thead>
            <tr className="text-xs text-[var(--color-on-surface-variant)]">
              <th className="py-2 text-start">حساب الإيراد</th>
              <th className="py-2 text-start">الوصف</th>
              <th className="py-2 text-end">المبلغ</th>
            </tr>
          </thead>
          <tbody>
            {lines.map((line) => (
              <tr key={line.id} className="border-t border-[var(--color-outline-variant)]">
                <td className="py-2">{line.revenueAccountId}</td>
                <td className="py-2">{line.description ?? '—'}</td>
                <td className="py-2 text-end"><MoneyDisplay value={line.amount ?? 0} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {checks.length > 0 && (
        <section aria-label="الشيكات" className="rounded-lg border border-[var(--color-outline-variant)] p-6">
          <h2 className="mb-3 text-sm font-medium text-[var(--color-on-surface)]">الشيكات</h2>
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
              {checks.map((check) => (
                <tr key={check.id} className="border-t border-[var(--color-outline-variant)]">
                  <td className="py-2">{check.bankName}</td>
                  <td className="py-2 tabular-nums">{check.checkNumber}</td>
                  <td className="py-2">{check.checkDate ? String(check.checkDate) : ''}</td>
                  <td className="py-2 text-end"><MoneyDisplay value={check.amount ?? 0} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}

      <div className="flex justify-end gap-3">
        {canSubmit && (
          <Button disabled={submitMutation.isPending} onClick={submit}>
            إرسال للمراجعة
          </Button>
        )}
        {canApprove && (
          <Button disabled={approveMutation.isPending} onClick={approve}>
            اعتماد
          </Button>
        )}
        {canCancel && (
          <Button variant="outline" onClick={() => setCancelOpen(true)}>
            إلغاء السند
          </Button>
        )}
      </div>

      <Dialog open={cancelOpen} onClose={() => setCancelOpen(false)} title="إلغاء السند — السبب مطلوب">
        <div className="space-y-4">
          <Textarea
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            rows={3}
            placeholder="اذكر سبب الإلغاء..."
          />
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={() => setCancelOpen(false)}>تراجع</Button>
            <Button variant="outline" disabled={!cancelReason.trim() || cancelMutation.isPending} onClick={cancel}>
              تأكيد الإلغاء
            </Button>
          </div>
        </div>
      </Dialog>
    </Page>
  );
}
