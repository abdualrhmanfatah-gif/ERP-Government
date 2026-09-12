import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { usePurchaseRequestDetail, useSubmitPurchaseRequest, useApprovePurchaseRequest, useRejectPurchaseRequest, useCancelPurchaseRequest } from '../hooks/usePurchaseRequests';
import { useItemsList, useUnitsList, useDepartmentsList, useCostCentersList } from '../shared/catalog-hooks';
import { purchaseRequestStatusLabels, purchaseRequestPriorityLabels, purchaseRequestStatusVariant, purchaseRequestPriorityVariant, type PurchaseRequestStatus, type PurchaseRequestPriority } from '../shared/types';
import { StatusLogPanel } from '@/components/DocumentsStatusLogPanel';
import { AttachmentsPanel } from '@/components/DocumentsAttachmentsPanel';
import { Page, Button, Card, Dialog, Textarea, ErrorState, StatusBadge, Badge } from '@/components/ui';
import { MetaItem } from '@/components/MetaItem';
import { ArrowRight, Send, CheckCircle, XCircle, Ban } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { PurchaseRequestForm } from '../components/PurchaseRequestForm';

export default function PurchaseRequestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = Number(id);
  const navigate = useNavigate();
  const { data: detail, isLoading, error, refetch } = usePurchaseRequestDetail(numericId);

  const { data: items } = useItemsList();
  const { data: units } = useUnitsList();
  const { data: departments } = useDepartmentsList();
  const { data: costCenters } = useCostCentersList();

  const submitMutation = useSubmitPurchaseRequest();
  const approveMutation = useApprovePurchaseRequest();
  const rejectMutation = useRejectPurchaseRequest();
  const cancelMutation = useCancelPurchaseRequest();

  const [dialogState, setDialogState] = useState<'reject' | 'cancel' | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [cancelReason, setCancelReason] = useState('');

  if (isLoading) return <Page title="طلب شراء" loading>{null}</Page>;
  if (error || !detail) return <Page title="طلب شراء"><ErrorState message="فشل تحميل تفاصيل طلب الشراء" onRetry={() => refetch()} /></Page>;

  const status = detail.status as string;
  const canSubmit = status === 'Draft';
  const canEdit = status === 'Draft';
  const canApprove = status === 'Submitted';
  const canReject = status === 'Submitted';
  const canCancel = status === 'Draft' || status === 'Approved';

  const handleSubmit = async () => {
    try {
      await submitMutation.mutateAsync(detail.id);
      notify({ type: 'success', title: 'تم تقديم طلب الشراء' });
      refetch();
    } catch (err) {
      handleLifecycleError(err);
    }
  };

  const handleApprove = async () => {
    try {
      await approveMutation.mutateAsync(detail.id);
      notify({ type: 'success', title: 'تم اعتماد طلب الشراء' });
      refetch();
    } catch (err) {
      handleLifecycleError(err);
    }
    setDialogState(null);
  };

  const handleReject = async () => {
    if (!rejectReason.trim()) {
      notify({ type: 'error', title: 'سبب الرفض مطلوب' });
      return;
    }
    try {
      await rejectMutation.mutateAsync({ id: detail.id, reason: rejectReason.trim() });
      notify({ type: 'success', title: 'تم رفض طلب الشراء' });
      refetch();
    } catch (err) {
      handleLifecycleError(err);
    }
    setDialogState(null);
    setRejectReason('');
  };

  const handleCancel = async () => {
    if (!cancelReason.trim()) {
      notify({ type: 'error', title: 'سبب الإلغاء مطلوب' });
      return;
    }
    try {
      await cancelMutation.mutateAsync({ id: detail.id, reason: cancelReason.trim() });
      notify({ type: 'success', title: 'تم إلغاء طلب الشراء' });
      refetch();
    } catch (err) {
      handleLifecycleError(err);
    }
    setDialogState(null);
    setCancelReason('');
  };

  const headerActions = (
    <div className="flex items-center gap-2 flex-wrap">
      {canEdit && (
        <Button variant="secondary" size="sm" onClick={() => navigate(`/procurement/purchase-requests/${detail.id}/edit`)}>
          تعديل
        </Button>
      )}
      {canSubmit && (
        <Button variant="primary" size="sm" onClick={handleSubmit} disabled={submitMutation.isPending} loading={submitMutation.isPending}>
          <Send className="h-4 w-4 ms-2" /> تقديم
        </Button>
      )}
      {canApprove && (
        <Button variant="primary" size="sm" onClick={handleApprove} disabled={approveMutation.isPending} loading={approveMutation.isPending}>
          <CheckCircle className="h-4 w-4 ms-2" /> اعتماد
        </Button>
      )}
      {canReject && (
        <Button variant="destructive" size="sm" onClick={() => setDialogState('reject')}>
          <XCircle className="h-4 w-4 ms-2" /> رفض
        </Button>
      )}
      {canCancel && (
        <Button variant="destructive" size="sm" onClick={() => setDialogState('cancel')}>
          <Ban className="h-4 w-4 ms-2" /> إلغاء
        </Button>
      )}
      {status === 'Approved' && (
        <Button variant="primary" size="sm" onClick={() => navigate(`/procurement/purchase-orders/create?purchaseRequestId=${detail.id}`)}>
          أمر شراء مباشر
        </Button>
      )}
      <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
        <ArrowRight size={18} />
      </Button>
    </div>
  );

  return (
    <Page
      title={`طلب شراء — ${detail.requestNumber}`}
      actions={headerActions}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-lg bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <StatusBadge variant={purchaseRequestStatusVariant[detail.status as PurchaseRequestStatus]}>
            {purchaseRequestStatusLabels[detail.status as PurchaseRequestStatus]}
          </StatusBadge>
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="رقم الطلب" value={detail.requestNumber} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="التاريخ" value={new Date(detail.requestDate).toLocaleDateString('ar-YE')} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="مقدم الطلب" value={detail.requesterName} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <Badge variant={purchaseRequestPriorityVariant[detail.priority as PurchaseRequestPriority]}>
            {purchaseRequestPriorityLabels[detail.priority as PurchaseRequestPriority]}
          </Badge>
        </div>
      }
    >
      {/* التخطيط الرئيسي: عمودان */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* العمود الأيسر: النموذج */}
        <div className="lg:col-span-8 space-y-6">
          <PurchaseRequestForm
            initialData={detail}
            readOnly
            onEdit={() => navigate(`/procurement/purchase-requests/${detail.id}/edit`)}
            onSubmit={() => {}}
            onCancel={() => navigate('/procurement/purchase-requests')}
            itemOptions={items ?? []}
            unitOptions={units ?? []}
            departmentOptions={departments ?? []}
            costCenterOptions={costCenters ?? []}
          />
        </div>

        {/* العمود الأيمن: المرفقات + سجل الحالات */}
        <div className="lg:col-span-4 space-y-6">
          <AttachmentsPanel documentType="PurchaseRequest" documentId={numericId} showGate={canSubmit || canApprove} />

          <Card>
            <div className="p-6"><StatusLogPanel documentType="PurchaseRequest" documentId={numericId} /></div>
          </Card>
        </div>
      </div>

      {/* نافذة الرفض */}
      <Dialog
        open={dialogState === 'reject'}
        onClose={() => { setDialogState(null); setRejectReason(''); }}
        title="رفض طلب الشراء"
        footer={
          <>
            <Button variant="outline" onClick={() => { setDialogState(null); setRejectReason(''); }}>إلغاء</Button>
            <Button variant="destructive" onClick={handleReject} disabled={rejectMutation.isPending || !rejectReason.trim()} loading={rejectMutation.isPending}>
              تأكيد الرفض
            </Button>
          </>
        }
      >
        <Textarea
          label="سبب الرفض"
          value={rejectReason}
          onChange={(e) => setRejectReason(e.target.value)}
          rows={3}
          placeholder="اذكر سبب الرفض..."
        />
      </Dialog>

      {/* نافذة الإلغاء */}
      <Dialog
        open={dialogState === 'cancel'}
        onClose={() => { setDialogState(null); setCancelReason(''); }}
        title="إلغاء طلب الشراء"
        footer={
          <>
            <Button variant="outline" onClick={() => { setDialogState(null); setCancelReason(''); }}>إلغاء</Button>
            <Button variant="destructive" onClick={handleCancel} disabled={cancelMutation.isPending || !cancelReason.trim()} loading={cancelMutation.isPending}>
              تأكيد الإلغاء
            </Button>
          </>
        }
      >
        <Textarea
          label="سبب الإلغاء"
          value={cancelReason}
          onChange={(e) => setCancelReason(e.target.value)}
          rows={3}
          placeholder="اذكر سبب الإلغاء..."
        />
      </Dialog>
    </Page>
  );
}
