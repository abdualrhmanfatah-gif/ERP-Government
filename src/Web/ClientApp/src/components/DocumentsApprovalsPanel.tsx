import { useState } from 'react';
import { useApprovals } from '@/features/documents/hooks/useDocuments';
import type { ApprovalsPanelProps } from '@/features/documents/shared/types';
import { Badge, Button } from '@/components/ui';

const decisionVariants: Record<string, 'success' | 'danger' | 'warning' | 'default'> = {
  Approved: 'success',
  Rejected: 'danger',
  Pending: 'warning',
};

export function ApprovalsPanel({ documentType, documentId }: ApprovalsPanelProps) {
  const { data: approvals, isLoading, isError, refetch } = useApprovals(documentType, documentId);
  const [isExpanded, setIsExpanded] = useState(true);

  if (isLoading) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
        <div className="h-6 w-32 rounded bg-[var(--color-surface-container)] animate-pulse" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
        <p className="text-sm text-[var(--color-error)]">حدث خطأ أثناء تحميل الاعتمادات</p>
        <Button variant="link" size="sm" onClick={() => refetch()} className="mt-2">
          إعادة المحاولة
        </Button>
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)]">
      <Button
        variant="ghost"
        className="w-full flex items-center justify-between px-4 py-3 text-sm font-semibold text-[var(--color-on-surface)]"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <span>اعتمادات</span>
        <span className="text-[var(--color-on-surface-variant)]">{isExpanded ? '▲' : '▼'}</span>
      </Button>
      {isExpanded && (
        <div className="px-4 pb-4">
          {!approvals || approvals.length === 0 ? (
            <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد اعتمادات بعد</p>
          ) : (
            <div className="space-y-3">
              {[...approvals].sort((a, b) => new Date(b.decisionAt).getTime() - new Date(a.decisionAt).getTime()).map((record) => (
                <div key={record.id} className="flex items-start gap-3 p-3 rounded bg-[var(--color-surface-container)]">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="text-sm font-medium">المستخدم #{record.approverUserId}</span>
                      <Badge variant={decisionVariants[record.decision] ?? 'default'}>
                        {record.decision}
                      </Badge>
                      <span className="text-xs text-[var(--color-on-surface-variant)]">
                        الخطوة {record.approvalStep}
                      </span>
                    </div>
                    <p className="text-xs text-[var(--color-on-surface-variant)]">
                      {new Date(record.decisionAt).toLocaleDateString('ar-YE')}
                    </p>
                    {record.reason && (
                      <p className="text-xs text-[var(--color-on-surface-variant)] mt-1">{record.reason}</p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default ApprovalsPanel;
