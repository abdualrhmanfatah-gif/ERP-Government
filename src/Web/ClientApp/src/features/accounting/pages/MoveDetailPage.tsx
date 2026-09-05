import { useParams } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { MoveDetail } from '../components/MoveDetail';
import { useMove } from '../hooks/useMoves';

export function MoveDetailPage() {
  const { id } = useParams<{ id: string }>();
  const moveId = Number(id);
  const { data: move, isLoading, error, refetch } = useMove(moveId);

  if (isLoading) {
    return (
      <div role="status" aria-busy="true" className="p-12 text-center text-[var(--color-on-surface-variant)]">
        جاري التحميل...
      </div>
    );
  }

  if (error || !move) {
    return (
      <div role="alert" className="p-12 text-center space-y-3">
        <p className="text-[var(--color-error)]">تعذر تحميل القيد</p>
        <p className="text-xs text-[var(--color-on-surface-variant)]">{(error as Error)?.message ?? 'غير موجود'}</p>
        <Button variant="outline" onClick={() => refetch()}>
          إعادة المحاولة
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <PageHeader title={`قيد ${move.entryNumber}`} description={move.narration ?? 'تفاصيل القيد وسطوره'} />
      <MoveDetail move={move} />
    </div>
  );
}
