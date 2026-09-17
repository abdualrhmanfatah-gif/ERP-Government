import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { RotateCcw } from 'lucide-react';
import { Page, Button, EmptyState, StatusBadge, Card, ConfirmDialog, MoneyDisplay, Input } from '@/components/ui';
import { useImpairmentDetail, useReverseImpairment } from '../hooks/useImpairments';
import { getImpairmentStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { formatDate } from '@/shared/utils/formatters';

export function ImpairmentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: imp, isLoading, error, refetch } = useImpairmentDetail(Number(id));
  const reverse = useReverseImpairment();
  const [confirmReverse, setConfirmReverse] = useState(false);
  const [reversalReason, setReversalReason] = useState('');

  if (isLoading) return <Page title="" loading>{null}</Page>;
  if (error) return <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>{null}</Page>;
  if (!imp) return <Page title="" onBack={() => navigate('/assets/impairments')}><EmptyState message="الإنخفاض غير موجود" /></Page>;

  const canReverse = imp.status === 'Posted' && !imp.isReversal;
  const badge = getImpairmentStatusBadge(imp.status);

  return (
    <Page title={`إنخفاض ${imp.documentNumber}`} description={<StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>}
      onBack={() => navigate('/assets/impairments')} maxWidth="md"
      actions={<div className="flex gap-2">
        {canReverse && <Button variant="destructive" icon={<RotateCcw size={14} />} onClick={() => setConfirmReverse(true)}>عكس</Button>}
      </div>}
    >
      <Card><div className="grid gap-4 md:grid-cols-2">
        <div><span className="text-[var(--color-on-surface-variant)]">الأصل: </span><span className="font-medium">{imp.assetCode} - {imp.assetName}</span></div>
        <div><span className="text-[var(--color-on-surface-variant)]">التاريخ: </span><span className="font-medium">{formatDate(imp.impairmentDate)}</span></div>
        <div><span className="text-[var(--color-on-surface-variant)]">القيمة السابقة: </span><MoneyDisplay value={imp.previousValue} /></div>
        <div><span className="text-[var(--color-on-surface-variant)]">القيمة الجديدة: </span><MoneyDisplay value={imp.newValue} /></div>
        <div><span className="text-[var(--color-on-surface-variant)]">مبلغ الإنخفاض: </span><MoneyDisplay value={imp.impairmentAmount} /></div>
        <div><span className="text-[var(--color-on-surface-variant)]">عكس: </span><span className="font-medium">{imp.isReversal ? 'نعم' : 'لا'}</span></div>
        {imp.reversalOfTransactionId && <div><span className="text-[var(--color-on-surface-variant)]">عكس لـ: </span><span dir="ltr">{imp.reversalOfTransactionId}</span></div>}
        {imp.reversalReason && <div className="md:col-span-2"><span className="text-[var(--color-on-surface-variant)]">سبب العكس: </span><span>{imp.reversalReason}</span></div>}
        {imp.notes && <div className="md:col-span-2"><span className="text-[var(--color-on-surface-variant)]">ملاحظات: </span><span>{imp.notes}</span></div>}
      </div></Card>

      <ConfirmDialog open={confirmReverse} onClose={() => setConfirmReverse(false)} title="عكس الإنخفاض" confirmLabel="عكس" destructive
        message={
          <div className="flex flex-col gap-3">
            <p>هل تريد عكس هذا الإنخفاض؟ سيتم إنشاء معاملة عكس مرتبطة.</p>
            <Input label="سبب العكس" value={reversalReason} onChange={(e) => setReversalReason(e.target.value)} required />
          </div>
        }
        onConfirm={async () => { try { await reverse.mutateAsync({ id: imp.id, data: { reversalReason, rowVersion: imp.rowVersion } }); setConfirmReverse(false); refetch(); } catch (err) { handleLifecycleError(err); } }} loading={reverse.isPending} />
    </Page>
  );
}
