import { useState } from 'react';
import { useStatusLog } from '@/features/documents/hooks/useDocuments';
import type { StatusLogPanelProps } from '@/features/documents/shared/types';
import { Button } from '@/components/ui/Button';

export function StatusLogPanel({ documentType, documentId }: StatusLogPanelProps) {
  const { data: statusLog, isLoading, isError, refetch } = useStatusLog(documentType, documentId);
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
        <p className="text-sm text-[var(--color-error)]">حدث خطأ أثناء تحميل سجل الحالات</p>
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
        <span>سجل الحالات</span>
        <span className="text-[var(--color-on-surface-variant)]">{isExpanded ? '▲' : '▼'}</span>
      </Button>
      {isExpanded && (
        <div className="px-4 pb-4">
          {!statusLog || statusLog.length === 0 ? (
            <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد تغييرات حالة مسجلة</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-[var(--color-outline-variant)]">
                    <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">من</th>
                    <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">إلى</th>
                    <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">بواسطة</th>
                    <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">التاريخ</th>
                    <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">السبب</th>
                  </tr>
                </thead>
                <tbody>
                  {[...statusLog].sort((a, b) => new Date(b.changedAt).getTime() - new Date(a.changedAt).getTime()).map((record) => (
                    <tr key={record.id} className="border-b border-[var(--color-outline-variant)] last:border-0">
                      <td className="px-3 py-3">{record.fromStatus}</td>
                      <td className="px-3 py-3">{record.toStatus}</td>
                      <td className="px-3 py-3">المستخدم #{record.changedById}</td>
                      <td className="px-3 py-3">{new Date(record.changedAt).toLocaleDateString('ar-YE')}</td>
                      <td className="px-3 py-3">{record.reason ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default StatusLogPanel;
