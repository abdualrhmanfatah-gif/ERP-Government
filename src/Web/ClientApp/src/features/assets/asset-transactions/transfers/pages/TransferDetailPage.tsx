import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Pencil, Play, Ban } from 'lucide-react';
import { Page, Button, EmptyState, StatusBadge, Card, ConfirmDialog } from '@/components/ui';
import { useTransferDetail, useExecuteTransfer, useCancelTransfer } from '../hooks/useTransfers';
import { getTransferStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { formatDate } from '@/shared/utils/formatters';

export function TransferDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: transfer, isLoading, error, refetch } = useTransferDetail(Number(id));
  const executeTransfer = useExecuteTransfer();
  const cancelTransfer = useCancelTransfer();
  const [confirmExecute, setConfirmExecute] = useState(false);
  const [confirmCancel, setConfirmCancel] = useState(false);

  if (isLoading) return <Page title="" loading>{null}</Page>;
  if (error) return <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>{null}</Page>;
  if (!transfer) return <Page title="" onBack={() => navigate('/assets/transfers')}><EmptyState message="النقل غير موجود" /></Page>;

  const isDraft = transfer.status === 'Draft';
  const badge = getTransferStatusBadge(transfer.status);

  return (
    <Page
      title={`نقل ${transfer.documentNumber}`}
      description={<StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>}
      onBack={() => navigate('/assets/transfers')}
      maxWidth="md"
      actions={
        <div className="flex gap-2">
          {isDraft && (
            <>
              <Button variant="ghost" icon={<Ban size={14} />} onClick={() => setConfirmCancel(true)}>إلغاء</Button>
              <Button variant="outline" icon={<Pencil size={14} />} onClick={() => navigate(`/assets/transfers/${transfer.id}/edit`)}>تعديل</Button>
              <Button variant="primary" icon={<Play size={14} />} onClick={() => setConfirmExecute(true)}>تنفيذ</Button>
            </>
          )}
        </div>
      }
    >
      <Card>
        <div className="grid gap-4 md:grid-cols-2">
          <div><span className="text-[var(--color-on-surface-variant)]">الأصل: </span><span className="font-medium">{transfer.assetCode} - {transfer.assetName}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">تاريخ النقل: </span><span className="font-medium">{formatDate(transfer.transactionDate)}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">من موقع: </span><span className="font-medium">{transfer.fromLocationName ?? '—'}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">إلى موقع: </span><span className="font-medium">{transfer.toLocationName ?? '—'}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">من حارس: </span><span className="font-medium">{transfer.fromEmployeeName ?? '—'}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">إلى حارس: </span><span className="font-medium">{transfer.toEmployeeName ?? '—'}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">من إدارة: </span><span className="font-medium">{transfer.fromDepartmentName ?? '—'}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">إلى إدارة: </span><span className="font-medium">{transfer.toDepartmentName ?? '—'}</span></div>
          {transfer.notes && <div className="md:col-span-2"><span className="text-[var(--color-on-surface-variant)]">ملاحظات: </span><span>{transfer.notes}</span></div>}
        </div>
      </Card>

      <ConfirmDialog
        open={confirmExecute}
        onClose={() => setConfirmExecute(false)}
        title="تنفيذ النقل"
        message="هل أنت متأكد من تنفيذ هذا النقل؟ سيتم تحديث بطاقة الأصل فوراً."
        confirmLabel="تنفيذ"
        onConfirm={async () => {
          try {
            await executeTransfer.mutateAsync({
              id: transfer.id,
              rowVersion: transfer.rowVersion,
              assetRowVersion: transfer.assetRowVersion,
            });
            setConfirmExecute(false);
            refetch();
          } catch (err) { handleLifecycleError(err); }
        }}
        loading={executeTransfer.isPending}
      />

      <ConfirmDialog
        open={confirmCancel}
        onClose={() => setConfirmCancel(false)}
        title="إلغاء النقل"
        message="هل أنت متأكد من إلغاء هذه المسودة؟ لن يحدث أي أثر على بطاقة الأصل."
        confirmLabel="إلغاء النقل"
        onConfirm={async () => {
          try {
            await cancelTransfer.mutateAsync({ id: transfer.id, rowVersion: transfer.rowVersion });
            setConfirmCancel(false);
            refetch();
          } catch (err) { handleLifecycleError(err); }
        }}
        loading={cancelTransfer.isPending}
      />
    </Page>
  );
}
