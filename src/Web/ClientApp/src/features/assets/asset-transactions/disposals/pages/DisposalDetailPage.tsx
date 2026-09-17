import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { CheckCircle, ArrowLeftRight } from 'lucide-react';
import { Page, Button, EmptyState, StatusBadge, Card, ConfirmDialog, MoneyDisplay } from '@/components/ui';
import { useDisposalDetail, useApproveDisposal, usePostDisposal } from '../hooks/useDisposals';
import { disposalMethodLabels } from '../shared/types';
import { getDisposalStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { formatDate } from '@/shared/utils/formatters';

export function DisposalDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: disposal, isLoading, error, refetch } = useDisposalDetail(Number(id));
  const approveDisposal = useApproveDisposal();
  const postDisposal = usePostDisposal();
  const [confirmApprove, setConfirmApprove] = useState(false);
  const [confirmPost, setConfirmPost] = useState(false);

  if (isLoading) return <Page title="" loading>{null}</Page>;
  if (error) return <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>{null}</Page>;
  if (!disposal) return <Page title="" onBack={() => navigate('/assets/disposals')}><EmptyState message="التخلص غير موجود" /></Page>;

  const canApprove = disposal.status === 'Draft';
  const canPost = disposal.status === 'Approved';
  const badge = getDisposalStatusBadge(disposal.status);

  return (
    <Page
      title={`تخلص ${disposal.documentNumber}`}
      description={<StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>}
      onBack={() => navigate('/assets/disposals')} maxWidth="md"
      actions={
        <div className="flex gap-2">
          {canApprove && <Button variant="primary" icon={<CheckCircle size={14} />} onClick={() => setConfirmApprove(true)}>اعتماد</Button>}
          {canPost && <Button variant="primary" icon={<ArrowLeftRight size={14} />} onClick={() => setConfirmPost(true)}>ترحيل</Button>}
        </div>
      }
    >
      <Card>
        <div className="grid gap-4 md:grid-cols-2">
          <div><span className="text-[var(--color-on-surface-variant)]">الأصل: </span><span className="font-medium">{disposal.assetCode} - {disposal.assetName}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">التاريخ: </span><span className="font-medium">{formatDate(disposal.disposalDate)}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">الطريقة: </span><span className="font-medium">{disposalMethodLabels[disposal.disposalMethod] ?? disposal.disposalMethod}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">القيمة الدفترية: </span><MoneyDisplay value={disposal.carryingAmount} /></div>
          <div><span className="text-[var(--color-on-surface-variant)]">المحصلات الصافية: </span>{disposal.netProceeds != null ? <MoneyDisplay value={disposal.netProceeds} /> : '—'}</div>
          <div><span className="text-[var(--color-on-surface-variant)]">الربح/الخسارة: </span>{disposal.gainOrLoss != null ? <MoneyDisplay value={disposal.gainOrLoss} /> : '—'}</div>
          {disposal.notes && <div className="md:col-span-2"><span className="text-[var(--color-on-surface-variant)]">ملاحظات: </span><span>{disposal.notes}</span></div>}
        </div>
      </Card>

      <ConfirmDialog open={confirmApprove} onClose={() => setConfirmApprove(false)} title="اعتماد التخلص" message="هل تريد اعتماد هذا التخلص؟" confirmLabel="اعتماد"
        onConfirm={async () => { try { await approveDisposal.mutateAsync({ id: disposal.id, rowVersion: disposal.rowVersion }); setConfirmApprove(false); refetch(); } catch (err) { handleLifecycleError(err); } }}
        loading={approveDisposal.isPending} />
      <ConfirmDialog open={confirmPost} onClose={() => setConfirmPost(false)} title="ترحيل التخلص" message="هل تريد ترحيل هذا التخلص؟ سيتم إنشاء قيد يومية." confirmLabel="ترحيل"
        onConfirm={async () => { try { await postDisposal.mutateAsync({ id: disposal.id, rowVersion: disposal.rowVersion }); setConfirmPost(false); refetch(); } catch (err) { handleLifecycleError(err); } }}
        loading={postDisposal.isPending} />
    </Page>
  );
}
