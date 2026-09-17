import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Play, CheckCircle, ClipboardCheck } from 'lucide-react';
import { Page, Button, EmptyState, StatusBadge, Card, ConfirmDialog, DataGrid } from '@/components/ui';
import { useCountDetail, useStartCount, useCompleteCount, useReviewCount } from '../hooks/useCounts';
import { isFoundLabels, type CountLine } from '../shared/types';
import { getCountStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { formatDate } from '@/shared/utils/formatters';

export function CountDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: count, isLoading, error, refetch } = useCountDetail(Number(id));
  const startCount = useStartCount();
  const completeCount = useCompleteCount();
  const reviewCount = useReviewCount();
  const [confirmStart, setConfirmStart] = useState(false);
  const [confirmComplete, setConfirmComplete] = useState(false);
  const [confirmReview, setConfirmReview] = useState(false);

  if (isLoading) return <Page title="" loading>{null}</Page>;
  if (error) return <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>{null}</Page>;
  if (!count) return <Page title="" onBack={() => navigate('/assets/counts')}><EmptyState message="الجرد غير موجود" /></Page>;

  const canStart = count.status === 'Draft';
  const canComplete = count.status === 'InProgress';
  const canReview = count.status === 'Completed';
  const badge = getCountStatusBadge(count.status);

  return (
    <Page title={`جرد ${count.documentNumber}`} description={<StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>}
      onBack={() => navigate('/assets/counts')} maxWidth="full"
      actions={<div className="flex gap-2">
        {canStart && <Button variant="primary" icon={<Play size={14} />} onClick={() => setConfirmStart(true)}>بدء</Button>}
        {canComplete && <Button variant="primary" icon={<CheckCircle size={14} />} onClick={() => setConfirmComplete(true)}>إكمال</Button>}
        {canReview && <Button variant="primary" icon={<ClipboardCheck size={14} />} onClick={() => setConfirmReview(true)}>مراجعة</Button>}
      </div>}
    >
      <Card><div className="grid gap-4 md:grid-cols-4">
        <div><span className="text-[var(--color-on-surface-variant)]">التاريخ: </span><span className="font-medium">{formatDate(count.countDate)}</span></div>
        <div><span className="text-[var(--color-on-surface-variant)]">النطاق: </span><span className="font-medium">{count.resolvedScopeLabel}</span></div>
        <div><span className="text-[var(--color-on-surface-variant)]">النوع: </span><span className="font-medium">{count.countType}</span></div>
        <div><span className="text-[var(--color-on-surface-variant)]">موجود/غير موجود/لم يفحص: </span><span className="font-medium">{count.foundCount}/{count.notFoundCount}/{count.notExaminedCount}</span></div>
      </div></Card>

      {count.lines.length > 0 && (
        <Card>
          <h3 className="mb-3 text-base font-semibold">بنود الجرد</h3>
          <DataGrid<CountLine> data={count.lines} rowKey={(r) => r.id} emptyMessage="لا توجد بنود"
            columns={[
              { id: 'assetCode', accessorKey: 'assetCode', header: 'كود الأصل', width: 120, cell: (r) => <span dir="ltr" className="tabular-nums font-mono">{r.assetCode}</span> },
              { id: 'assetName', accessorKey: 'assetName', header: 'الأصل' },
              { id: 'isFound', accessorKey: 'isFound', header: 'الحالة', width: 120, cell: (r) => <StatusBadge variant={r.isFound === 1 ? 'active' : r.isFound === 2 ? 'closed' : 'draft'}>{isFoundLabels[r.isFound]}</StatusBadge> },
              { id: 'physicalLocation', accessorKey: 'physicalLocation', header: 'الموقع الفعلي', cell: (r) => r.physicalLocation ?? '—' },
              { id: 'discrepancyNotes', accessorKey: 'discrepancyNotes', header: 'ملاحظات', cell: (r) => r.discrepancyNotes ?? '—' },
            ]} />
        </Card>
      )}

      <ConfirmDialog open={confirmStart} onClose={() => setConfirmStart(false)} title="بدء الجرد" message="هل تريد بدء هذا الجرد؟ سيتم توليد البنود وتثبيت النطاق." confirmLabel="بدء"
        onConfirm={async () => { try { await startCount.mutateAsync(count.id); setConfirmStart(false); refetch(); } catch (err) { handleLifecycleError(err); } }} loading={startCount.isPending} />
      <ConfirmDialog open={confirmComplete} onClose={() => setConfirmComplete(false)} title="إكمال الجرد" message="هل تريد إكمال هذا الجرد؟ يجب أن تكون جميع البنود مفحوصة." confirmLabel="إكمال"
        onConfirm={async () => { try { await completeCount.mutateAsync(count.id); setConfirmComplete(false); refetch(); } catch (err) { handleLifecycleError(err); } }} loading={completeCount.isPending} />
      <ConfirmDialog open={confirmReview} onClose={() => setConfirmReview(false)} title="مراجعة الجرد" message="هل تريد مراجعة هذا الجرد؟" confirmLabel="مراجعة"
        onConfirm={async () => { try { await reviewCount.mutateAsync(count.id); setConfirmReview(false); refetch(); } catch (err) { handleLifecycleError(err); } }} loading={reviewCount.isPending} />
    </Page>
  );
}
