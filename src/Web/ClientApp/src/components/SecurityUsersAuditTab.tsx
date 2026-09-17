import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/Button';
import { usersAuditClient } from '@/features/security/users/client';
import { getQueryErrorMessage } from '@/shared/api/query-error';

interface AuditTabProps {
  userId: number;
}

export function AuditTab({ userId }: AuditTabProps) {
  const { data: entries = [], isLoading, isError: isEntriesError, error: entriesError, refetch: refetchEntries } = useQuery({
    queryKey: ['users', userId, 'audit'],
    queryFn: () => usersAuditClient.list(userId),
    enabled: userId > 0,
  });

  if (isLoading) {
    return <div className="text-[var(--color-on-surface-variant)] p-4">جاري التحميل...</div>;
  }

  if (isEntriesError) {
    return (
      <div className="p-4 text-center flex flex-col items-center gap-2">
        <p className="text-sm text-[var(--color-error)]">{getQueryErrorMessage(entriesError)}</p>
        <Button variant="outline" size="sm" onClick={() => refetchEntries()}>إعادة المحاولة</Button>
      </div>
    );
  }

  if (entries.length === 0) {
    return <div className="text-[var(--color-on-surface-variant)] text-sm p-4">لا يوجد سجل تغييرات</div>;
  }

  return (
    <div className="space-y-3">
      {entries.map((entry) => (
        <div key={entry.id} className="border border-[var(--color-outline)] rounded-lg p-3 text-sm">
          <div className="flex justify-between items-center mb-2">
            <span className="font-medium">{entry.action}</span>
            <span className="text-[var(--color-on-surface-variant)] text-xs">
              {new Date(entry.timestamp).toLocaleString('ar')}
            </span>
          </div>
          <div className="text-[var(--color-on-surface-variant)] text-xs mb-1">
            بواسطة: {entry.actor}
          </div>
          {entry.oldValues && (
            <details className="mt-2">
              <summary className="text-xs text-[var(--color-on-surface-variant)] cursor-pointer">عرض التفاصيل</summary>
              <pre className="mt-1 text-xs bg-[var(--color-surface)] p-2 rounded-lg overflow-auto max-h-40" dir="ltr">
                {(() => {
                  try {
                    const old = JSON.parse(entry.oldValues);
                    const newV = entry.newValues ? JSON.parse(entry.newValues) : {};
                    return JSON.stringify({ from: old, to: newV }, null, 2);
                  } catch {
                    return entry.oldValues;
                  }
                })()}
              </pre>
            </details>
          )}
        </div>
      ))}
    </div>
  );
}
