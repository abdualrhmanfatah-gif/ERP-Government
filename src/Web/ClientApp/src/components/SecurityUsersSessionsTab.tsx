import { notify } from '@/features/notifications/notify';
import { Trash2, LogOut } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { DataGrid } from '@/components/ui/DataGrid';
import { useUserSessions, useRevokeUserSession, useRevokeAllUserSessions } from '@/features/security/users/hooks';
import type { UserSessionDto } from '@/features/security/users/types';

interface SessionsTabProps {
  userId: number;
}

export function SessionsTab({ userId }: SessionsTabProps) {
  const { data: sessions = [], isLoading } = useUserSessions(userId);
  const revokeSession = useRevokeUserSession();
  const revokeAll = useRevokeAllUserSessions();

  async function handleRevoke(sessionId: number) {
    try {
      await revokeSession.mutateAsync({ userId, sessionId });
      notify({ type: 'success', title: 'تم إلغاء الجلسة بنجاح' });
    } catch (err) {
      notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ' });
    }
  }

  async function handleRevokeAll() {
    if (!confirm('هل تريد إلغاء جميع الجلسات النشطة؟')) return;
    try {
      await revokeAll.mutateAsync(userId);
      notify({ type: 'success', title: 'تم إلغاء جميع الجلسات بنجاح' });
    } catch (err) {
      notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ' });
    }
  }

  const columns = [
    { key: 'userAgent', header: 'الجهاز', accessorKey: 'userAgent' as const, width: 200, render: (r: UserSessionDto) => r.userAgent ?? '—' },
    { key: 'ipAddress', header: 'عنوان IP', accessorKey: 'ipAddress' as const, width: 120 },
    { key: 'createdAt', header: 'تاريخ الدخول', accessorKey: 'createdAt' as const, width: 150, render: (r: UserSessionDto) => new Date(r.createdAt).toLocaleString('ar') },
    { key: 'lastActivityAt', header: 'آخر نشاط', accessorKey: 'lastActivityAt' as const, width: 150, render: (r: UserSessionDto) => r.lastActivityAt ? new Date(r.lastActivityAt).toLocaleString('ar') : '—' },
    { key: 'actions', header: 'الإجراءات', width: 80, render: (r: UserSessionDto) => (
      <Button variant="ghost" size="icon-xs" onClick={() => handleRevoke(r.id)} title="إلغاء">
        <Trash2 size={14} className="text-[var(--color-error)]" />
      </Button>
    )},
  ];

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button variant="ghost" size="sm" icon={<LogOut size={14} />} onClick={handleRevokeAll} disabled={sessions.length === 0}>
          إلغاء الكل
        </Button>
      </div>

      <DataGrid
        columns={columns}
        data={sessions}
        loading={isLoading}
        emptyMessage="لا توجد جلسات نشطة"
        rowKey={(r) => r.id}
      />
    </div>
  );
}
