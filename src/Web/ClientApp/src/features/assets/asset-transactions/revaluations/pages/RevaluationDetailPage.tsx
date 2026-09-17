import { useParams, useNavigate } from 'react-router-dom';
import { Page, EmptyState, StatusBadge, Card, MoneyDisplay } from '@/components/ui';
import { useRevaluationDetail } from '../hooks/useRevaluations';
import { getRevaluationStatusBadge } from '../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';

export function RevaluationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: rev, isLoading, error, refetch } = useRevaluationDetail(Number(id));

  if (isLoading) return <Page title="" loading>{null}</Page>;
  if (error) return <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>{null}</Page>;
  if (!rev) return <Page title="" onBack={() => navigate('/assets/revaluations')}><EmptyState message="التقييم غير موجود" /></Page>;

  const badge = getRevaluationStatusBadge(rev.status);

  return (
    <Page title={`تقييم ${rev.documentNumber}`} description={<StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>}
      onBack={() => navigate('/assets/revaluations')} maxWidth="md"
    >
      <Card><div className="grid gap-4 md:grid-cols-2">
        <div><span className="text-[var(--color-on-surface-variant)]">الأصل: </span><span className="font-medium">{rev.assetCode} - {rev.assetName}</span></div>
        <div><span className="text-[var(--color-on-surface-variant)]">التاريخ: </span><span className="font-medium">{formatDate(rev.revaluationDate)}</span></div>
        <div><span className="text-[var(--color-on-surface-variant)]">القيمة السابقة: </span><MoneyDisplay value={rev.previousValue} /></div>
        <div><span className="text-[var(--color-on-surface-variant)]">القيمة الجديدة: </span><MoneyDisplay value={rev.newValue} /></div>
        <div><span className="text-[var(--color-on-surface-variant)]">مبلغ التقييم: </span><MoneyDisplay value={rev.revaluationAmount} /></div>
        <div><span className="text-[var(--color-on-surface-variant)]">النوع: </span><span className="font-medium">{rev.type}</span></div>
        {rev.notes && <div className="md:col-span-2"><span className="text-[var(--color-on-surface-variant)]">ملاحظات: </span><span>{rev.notes}</span></div>}
      </div></Card>
    </Page>
  );
}
