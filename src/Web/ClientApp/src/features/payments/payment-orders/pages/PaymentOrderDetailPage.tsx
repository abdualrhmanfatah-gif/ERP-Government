import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  usePaymentOrderDetail,
  usePaymentOrderTotals,
  useSubmitPaymentOrder,
  useApprovePaymentOrder,
  useRejectPaymentOrder,
  useCancelPaymentOrder,
  useSendToTreasuryPaymentOrder,
  useVoidPaymentOrder,
} from '../hooks/usePaymentOrders';
import { PaymentOrderStatus } from '../shared/types';
import { PaymentsStatusBadge } from '@/components/PaymentsStatusBadge';
import { RecordPaymentDialog } from '@/features/payments/payments/pages/RecordPaymentDialog';
import { PaymentOrderForm } from '@/components/PaymentOrderForm';
import { useFundsList } from '@/features/budgeting/funds/hooks/useFunds';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { Page, Button, Card, ConfirmDialog, Combobox, Input } from '@/components/ui';
import { ArrowRight, ExternalLink } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { usePermission } from '@/shared/hooks/usePermission';

const canSubmit = (s: PaymentOrderStatus) => s === 'Draft';
const canApprove = (s: PaymentOrderStatus) => s === 'Submitted';
const canReject = (s: PaymentOrderStatus) => s === 'Submitted';
const canCancel = (s: PaymentOrderStatus, isFullyPaid: boolean) =>
  (s === 'Draft' || s === 'Submitted') && !isFullyPaid;
const canSendToTreasury = (s: PaymentOrderStatus) => s === 'Approved';
const canVoid = (s: PaymentOrderStatus, isFullyPaid: boolean) =>
  (s === 'Approved' || s === 'SentToTreasury') && !isFullyPaid;
const canRecordPayment = (s: PaymentOrderStatus, paidAmount: number) =>
  (s === 'Approved' || s === 'SentToTreasury') && paidAmount === 0;
const canEdit = (s: PaymentOrderStatus) => s === 'Draft';

export default function PaymentOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const orderId = Number(id);

  const [editing, setEditing] = useState(false);
  const [showRecordPayment, setShowRecordPayment] = useState(false);
  const [showSubmitDialog, setShowSubmitDialog] = useState(false);
  const [dialogState, setDialogState] = useState<'approve' | 'reject' | 'cancel' | 'void' | 'sendToTreasury' | null>(null);
  const [reason, setReason] = useState('');
  const [submitFundId, setSubmitFundId] = useState<number>(0);
  const [submitAccountId, setSubmitAccountId] = useState<number>(0);

  const { data: order, isLoading, refetch } = usePaymentOrderDetail(orderId);
  const { data: totals } = usePaymentOrderTotals(orderId);
  const { data: funds = [] } = useFundsList(true);
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });

  const submitMutation = useSubmitPaymentOrder();
  const approveMutation = useApprovePaymentOrder();
  const rejectMutation = useRejectPaymentOrder();
  const cancelMutation = useCancelPaymentOrder();
  const sendToTreasuryMutation = useSendToTreasuryPaymentOrder();
  const voidMutation = useVoidPaymentOrder();

  const { hasPermission: canCreatePayment } = usePermission('Payments.Create' as never);
  const { hasPermission: canApproveOrder } = usePermission('PaymentOrders.Approve' as never);
  const { hasPermission: canSendToTreasuryPerm } = usePermission('PaymentOrders.SendToTreasury' as never);
  const { hasPermission: canVoidPerm } = usePermission('PaymentOrders.Void' as never);
  const { hasPermission: canCancelPerm } = usePermission('PaymentOrders.Cancel' as never);

  console.log('[PaymentOrderDetail] Permissions:', {
    canCreatePayment,
    canApproveOrder,
    canSendToTreasuryPerm,
    canVoidPerm,
    canCancelPerm,
  });

  console.log('[PaymentOrderDetail] isLoading:', isLoading, '| order:', order, '| orderId:', orderId);

  if (isLoading) return <Page title="جارٍ التحميل..." loading />;
  if (!order) return <Page title="لم يتم العثور على أمر الدفع" />;

  const status = order.status as PaymentOrderStatus;
  const isFullyPaid = totals?.isFullyPaid ?? false;
  const netAmount = (order.amountGross ?? 0) - (order.deductionAmount ?? 0);

  console.log('[PaymentOrderDetail] status:', status, '| typeof:', typeof status);
  console.log('[PaymentOrderDetail] canSendToTreasury:', status === 'Approved');
  console.log('[PaymentOrderDetail] order:', { id: order.id, status: order.status, rowVersion: order.rowVersion });
  console.log('[PaymentOrderDetail] totals:', totals);
  console.log('[PaymentOrderDetail] Button conditions:', {
    canEdit: status === 'Draft',
    canSubmit: status === 'Draft',
    canApprove: status === 'Submitted' && canApproveOrder,
    canReject: status === 'Submitted' && canApproveOrder,
    canCancel: (status === 'Draft' || status === 'Submitted') && !isFullyPaid && canCancelPerm,
    canSendToTreasury: status === 'Approved' && canSendToTreasuryPerm,
    canVoid: (status === 'Approved' || status === 'SentToTreasury') && !isFullyPaid && canVoidPerm,
    canRecordPayment: (status === 'Approved' || status === 'SentToTreasury') && (totals?.paidAmount ?? 0) === 0 && canCreatePayment,
  });

  async function handleAction() {
    try {
      switch (dialogState) {
        case 'approve':
          await approveMutation.mutateAsync({ id: orderId, rowVersion: order!.rowVersion ?? '' });
          notify({ type: 'success', title: 'تمت الموافقة على أمر الدفع' });
          break;
        case 'reject':
          await rejectMutation.mutateAsync({ id: orderId, rejectionReason: reason, rowVersion: order!.rowVersion ?? '' });
          notify({ type: 'success', title: 'تم رفض أمر الدفع' });
          break;
        case 'cancel':
          await cancelMutation.mutateAsync({ id: orderId, cancellationReason: reason, rowVersion: order!.rowVersion ?? '' });
          notify({ type: 'success', title: 'تم إلغاء أمر الدفع' });
          break;
        case 'void':
          await voidMutation.mutateAsync({ id: orderId, voidReason: reason, rowVersion: order!.rowVersion ?? '' });
          notify({ type: 'success', title: 'تم إبطال أمر الدفع' });
          break;
        case 'sendToTreasury':
          await sendToTreasuryMutation.mutateAsync({ id: orderId, rowVersion: order!.rowVersion ?? '' });
          notify({ type: 'success', title: 'تم إرسال أمر الدفع للخزينة' });
          break;
      }
      refetch();
    } catch (err: unknown) {
      let detail = '';
      if (err instanceof Error && 'response' in err) {
        const body = (err as { response: string }).response;
        try { detail = JSON.parse(body).join(', '); } catch { detail = body || err.message; }
      } else if (err instanceof Error) {
        detail = err.message;
      }
      notify({ type: 'error', title: detail || 'حدث خطأ' });
    }
    setDialogState(null);
    setReason('');
  }

  async function handleSubmit() {
    try {
      await submitMutation.mutateAsync({
        id: orderId,
        fundId: submitFundId || order!.fundId || 0,
        budgetItemAllocationId: order!.budgetItemAllocationId || 0,
        accountId: submitAccountId || order!.accountId || undefined,
        rowVersion: order!.rowVersion ?? '',
      });
      notify({ type: 'success', title: 'تم تقديم أمر الدفع' });
      setShowSubmitDialog(false);
      refetch();
    } catch (err: unknown) {
      let detail = '';
      if (err instanceof Error && 'response' in err) {
        const body = (err as { response: string }).response;
        try { detail = JSON.parse(body).join(', '); } catch { detail = body || err.message; }
      } else if (err instanceof Error) {
        detail = err.message;
      }
      notify({ type: 'error', title: detail || 'حدث خطأ' });
    }
  }

  const headerActions = !editing ? (
    <div className="flex items-center gap-2 flex-wrap">
      {canEdit(status) && (
        <Button variant="secondary" size="sm" onClick={() => setEditing(true)}>تعديل</Button>
      )}
      {canSubmit(status) && (
        <Button variant="default" size="sm" onClick={() => {
          setSubmitFundId(order!.fundId ?? 0);
          setSubmitAccountId(order!.accountId ?? 0);
          setShowSubmitDialog(true);
        }} disabled={submitMutation.isPending}>تقديم</Button>
      )}
      {canApprove(status) && canApproveOrder && (
        <Button variant="success" size="sm" onClick={() => setDialogState('approve')}>موافقة</Button>
      )}
      {canReject(status) && canApproveOrder && (
        <Button variant="destructive" size="sm" onClick={() => setDialogState('reject')}>رفض</Button>
      )}
      {canCancel(status, isFullyPaid) && canCancelPerm && (
        <Button variant="destructive" size="sm" onClick={() => setDialogState('cancel')}>إلغاء</Button>
      )}
      {canSendToTreasury(status) && canSendToTreasuryPerm && (
        <Button variant="info" size="sm" onClick={() => setDialogState('sendToTreasury')}>إرسال للخزينة</Button>
      )}
      {canVoid(status, isFullyPaid) && canVoidPerm && (
        <Button variant="destructive" size="sm" onClick={() => setDialogState('void')}>إبطال</Button>
      )}
      {canRecordPayment(status, totals?.paidAmount ?? 0) && canCreatePayment && (
        <Button variant="success" size="sm" onClick={() => setShowRecordPayment(true)}>تسجيل الدفع</Button>
      )}
    </div>
  ) : null;

  return (
    <Page
      title={`أمر دفع — ${order.paymentOrderNumber ?? ''}`}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-lg bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <PaymentsStatusBadge status={status} variant="order" />
        </div>
      }
      actions={
        <div className="flex items-center gap-2">
          {headerActions}
          <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
            <ArrowRight size={18} />
          </Button>
        </div>
      }
    >
      <div className="space-y-6">
        {/* Payment Order Form — read-only or editable */}
        <PaymentOrderForm
          mode="detail"
          initialData={order}
          editing={editing}
          onToggleEditing={setEditing}
          onSaved={() => { refetch(); setEditing(false); }}
          onCancel={() => setEditing(false)}
        />

        {/* Financial summary */}
        {totals && (
          <Card>
            <h2 className="text-sm font-semibold mb-3">الملخص المالي</h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
              <div>
                <span className="text-[var(--color-on-surface-variant)]">الإجمالي:</span>{' '}
                {(totals.amountGross ?? 0).toLocaleString('ar-YE')}
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">الخصومات:</span>{' '}
                {(totals.totalDeductions ?? 0).toLocaleString('ar-YE')}
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">الصافي:</span>{' '}
                {(totals.netAmount ?? 0).toLocaleString('ar-YE')}
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">المدفوع:</span>{' '}
                {(totals.paidAmount ?? 0).toLocaleString('ar-YE')}
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">المتبقي:</span>{' '}
                {(totals.remainingAmount ?? 0).toLocaleString('ar-YE')}
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">مدفوع بالكامل:</span>{' '}
                {totals.isFullyPaid ? 'نعم' : 'لا'}
              </div>
            </div>
          </Card>
        )}

        {/* Journal tracking */}
        {order.journalEntryId && (
          <Card>
            <h2 className="text-sm font-semibold mb-3">تتبع القيد</h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
              {order.journalEntryId && (
                <div>
                  <span className="text-[var(--color-on-surface-variant)]">القيد المحاسبي:</span>{' '}
                  <Button
                    variant="link"
                    size="sm"
                    className="inline-flex items-center gap-1 p-0 h-auto"
                    onClick={() => navigate(`/accounting/journal-entries/${order.journalEntryId}`)}
                  >
                    {order.journalEntryId} <ExternalLink size={12} />
                  </Button>
                </div>
              )}
            </div>
          </Card>
        )}
      </div>

      {/* Dialogs */}
      {showRecordPayment && (
        <RecordPaymentDialog
          open={showRecordPayment}
          onOpenChange={setShowRecordPayment}
          paymentOrderId={orderId}
          amount={netAmount}
        />
      )}

      <ConfirmDialog
        open={dialogState === 'approve'}
        onClose={() => { setDialogState(null); setReason(''); }}
        onConfirm={handleAction}
        title="موافقة على أمر الدفع"
        message="هل أنت متأكد من الموافقة على أمر الدفع؟"
        confirmLabel="تأكيد الموافقة"
      />
      <ConfirmDialog
        open={dialogState === 'reject'}
        onClose={() => { setDialogState(null); setReason(''); }}
        onConfirm={handleAction}
        title="رفض أمر الدفع"
        message={
          <div className="space-y-2">
            <p>هل أنت متأكد من رفض أمر الدفع؟</p>
            <Input label="سبب الرفض" value={reason} onChange={(e) => setReason(e.target.value)} required />
          </div>
        }
        confirmLabel="تأكيد الرفض"
        destructive
      />
      <ConfirmDialog
        open={dialogState === 'cancel'}
        onClose={() => { setDialogState(null); setReason(''); }}
        onConfirm={handleAction}
        title="إلغاء أمر الدفع"
        message={
          <div className="space-y-2">
            <p>هل أنت متأكد من إلغاء أمر الدفع؟ سيتم إبطال أي طلبات صرف مرتبطة به.</p>
            <Input label="سبب الإلغاء" value={reason} onChange={(e) => setReason(e.target.value)} required />
          </div>
        }
        confirmLabel="تأكيد الإلغاء"
        destructive
      />
      <ConfirmDialog
        open={dialogState === 'void'}
        onClose={() => { setDialogState(null); setReason(''); }}
        onConfirm={handleAction}
        title="إبطال أمر الدفع"
        message={
          <div className="space-y-2">
            <p>هل أنت متأكد من إبطال أمر الدفع؟ لا يمكن التراجع عن هذا الإجراء.</p>
            <Input label="سبب الإبطال" value={reason} onChange={(e) => setReason(e.target.value)} required />
          </div>
        }
        confirmLabel="تأكيد الإبطال"
        destructive
      />
      <ConfirmDialog
        open={dialogState === 'sendToTreasury'}
        onClose={() => { setDialogState(null); setReason(''); }}
        onConfirm={handleAction}
        title="إرسال أمر الدفع للخزينة"
        message="هل أنت متأكد من إرسال أمر الدفع للخزينة؟"
        confirmLabel="تأكيد الإرسال"
      />
      <ConfirmDialog
        open={showSubmitDialog}
        onClose={() => setShowSubmitDialog(false)}
        onConfirm={handleSubmit}
        title="تقديم أمر الدفع"
        message={
          <div className="space-y-3">
            <p>اختر الصندوق والحساب لتقديم أمر الدفع:</p>
            <Combobox
              label="الصندوق"
              value={submitFundId ? String(submitFundId) : ''}
              onChange={(val) => setSubmitFundId(val ? Number(val) : 0)}
              options={funds.map((f) => ({ value: String(f.id), label: `${f.fundNumber} — ${f.fundName}` }))}
              placeholder="اختر صندوق..."
              searchPlaceholder="بحث بالرقم أو الاسم..."
              emptyMessage="لا توجد نتائج"
            />
            <Combobox
              label="الحساب"
              value={submitAccountId ? String(submitAccountId) : ''}
              onChange={(val) => setSubmitAccountId(val ? Number(val) : 0)}
              options={accounts.map((a) => ({ value: String(a.id), label: `${a.code} — ${a.name}` }))}
              placeholder="اختر حساب..."
              searchPlaceholder="بحث برقم أو اسم الحساب..."
              emptyMessage="لا توجد نتائج"
            />
          </div>
        }
        confirmLabel="تقديم"
      />
    </Page>
  );
}
