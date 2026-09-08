import { usePendingApprovalsCount } from '@/features/dashboard/hooks/usePendingApprovalsCount';
import { ErrorState } from '@/components/ui/ErrorState';
import { EmptyState } from '@/components/ui/EmptyState';
import { Button } from '@/components/ui/Button';
import { useNavigate } from 'react-router-dom';
import { Clock, ChevronLeft } from 'lucide-react';

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ar-SA', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }).format(date);
}

export function PendingApprovalsList() {
  const navigate = useNavigate();
  const { data, isLoading, error, refetch } = usePendingApprovalsCount();

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="h-16 bg-[var(--color-surface-container-low)] rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  if (error) {
    return <ErrorState message={error.message} onRetry={refetch} />;
  }

  if (!data || data.items.length === 0) {
    return <EmptyState message="لا توجد موافقات معلقة" />;
  }

  return (
    <div className="space-y-3">
      {data.items.slice(0, 5).map((item) => (
        <div
          key={item.id}
          className="flex items-center justify-between p-3 rounded-lg border border-[var(--color-border-container)] hover:bg-[var(--color-surface-container-low)] transition-colors cursor-pointer"
          onClick={() => navigate('/approval-rules/pending')}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              navigate('/approval-rules/pending');
            }
          }}
          tabIndex={0}
          role="button"
          aria-label={`عرض تفاصيل الموافقة: ${item.title}`}
        >
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-[var(--color-warning-bg)] flex items-center justify-center">
              <Clock size={18} className="text-[var(--color-warning)]" />
            </div>
            <div>
              <div className="text-body-sm font-medium text-[var(--color-on-surface)]">{item.title}</div>
              <div className="text-label-sm text-[var(--color-on-surface-variant)]">
                {formatDate(item.createdAt)}
              </div>
            </div>
          </div>
          <ChevronLeft size={16} className="text-[var(--color-on-surface-variant)]" />
        </div>
      ))}
      {data.items.length > 5 && (
        <Button
          variant="link"
          size="sm"
          onClick={() => navigate('/approval-rules/pending')}
          className="w-full"
        >
          عرض الكل ({data.items.length})
        </Button>
      )}
    </div>
  );
}
