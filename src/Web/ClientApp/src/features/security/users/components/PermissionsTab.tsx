import { useState } from 'react';
import { notify } from '@/features/notifications/notify';
import { Trash2, Plus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { DataGrid } from '@/components/ui/DataGrid';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Dialog } from '@/components/ui/Dialog';
import { useUserPermissions, useAssignUserPermission, useRemoveUserPermission } from '../hooks';
import { permissionsClient } from '../../rbac/client';
import { useQuery } from '@tanstack/react-query';
import type { EffectivePermissionDto } from '../types';

interface PermissionsTabProps {
  userId: number;
}

type PendingAction =
  | { type: 'grant'; permissionId: number }
  | { type: 'revoke'; permissionId: number; source: string }
  | null;

export function PermissionsTab({ userId }: PermissionsTabProps) {
  const { data: permissions = [], isLoading } = useUserPermissions(userId);
  const assignPermission = useAssignUserPermission();
  const removePermission = useRemoveUserPermission();
  const [selectedPermissionId, setSelectedPermissionId] = useState('');
  const [pendingAction, setPendingAction] = useState<PendingAction>(null);
  const [reason, setReason] = useState('');

  const { data: allPermissions = [] } = useQuery({
    queryKey: ['rbac-permissions'],
    queryFn: () => permissionsClient.list(),
  });

  const grantedIds = new Set(permissions.filter((p) => p.isGranted).map((p) => p.permissionId));
  const availablePermissions = allPermissions.filter((p) => p.isActive && !grantedIds.has(p.id));

  function openGrantDialog() {
    if (!selectedPermissionId) return;
    setPendingAction({ type: 'grant', permissionId: Number(selectedPermissionId) });
    setReason('');
  }

  function openRevokeDialog(permissionId: number, source: string) {
    setPendingAction({ type: 'revoke', permissionId, source });
    setReason('');
  }

  async function confirmAction() {
    if (!pendingAction || !reason.trim()) return;
    try {
      if (pendingAction.type === 'grant') {
        await assignPermission.mutateAsync({
          userId,
          data: { userId, permissionId: pendingAction.permissionId, isGranted: true, reason: reason.trim() },
        });
        notify({ type: 'success', title: 'تم تعيين الصلاحية بنجاح' });
        setSelectedPermissionId('');
      } else {
        if (pendingAction.source === 'override') {
          await removePermission.mutateAsync({ userId, permissionId: pendingAction.permissionId });
        } else {
          await assignPermission.mutateAsync({
            userId,
            data: { userId, permissionId: pendingAction.permissionId, isGranted: false, reason: reason.trim() },
          });
        }
        notify({ type: 'success', title: 'تم إزالة الصلاحية بنجاح' });
      }
      setPendingAction(null);
      setReason('');
    } catch (err) {
      notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ' });
    }
  }

  const columns = [
    { key: 'code', header: 'الكود', width: 160, render: (r: EffectivePermissionDto) => <span dir="ltr">{r.code}</span> },
    { key: 'name', header: 'الاسم', width: 200, render: (r: EffectivePermissionDto) => r.name },
    {
      key: 'source', header: 'المصدر', width: 100, render: (r: EffectivePermissionDto) => (
        <StatusBadge variant={r.source === 'role' ? 'active' : 'draft'}>
          {r.source === 'role' ? 'الدور' : 'استثناء'}
        </StatusBadge>
      ),
    },
    {
      key: 'isGranted', header: 'الحالة', width: 100, render: (r: EffectivePermissionDto) => (
        <StatusBadge variant={r.isGranted ? 'active' : 'closed'}>
          {r.isGranted ? 'ممنوح' : 'مرفوض'}
        </StatusBadge>
      ),
    },
    { key: 'reason', header: 'السبب', width: 180, render: (r: EffectivePermissionDto) => r.reason ?? '—' },
    {
      key: 'actions', header: 'الإجراءات', width: 80, render: (r: EffectivePermissionDto) => (
        <Button variant="ghost" size="icon-xs" onClick={() => openRevokeDialog(r.permissionId, r.source)} title="إزالة">
          <Trash2 size={14} className="text-[var(--color-error)]" />
        </Button>
      ),
    },
  ];

  const actionLabel = pendingAction?.type === 'grant' ? 'منح' : 'إزالة';

  return (
    <div className="space-y-4">
      <div className="flex gap-3 items-end">
        <div className="flex-1 max-w-xs">
          <Select
            label="تعيين صلاحية"
            value={selectedPermissionId}
            onChange={(e) => setSelectedPermissionId(e.target.value)}
            options={[
              { value: '', label: '— اختر صلاحية —' },
              ...availablePermissions.map((p) => ({ value: String(p.id), label: `${p.code} — ${p.name}` })),
            ]}
          />
        </div>
        <Button
          variant="primary"
          size="sm"
          icon={<Plus size={14} />}
          onClick={openGrantDialog}
          disabled={!selectedPermissionId}
          className="mb-6"
        >
          تعيين
        </Button>
      </div>

      <DataGrid
        columns={columns}
        data={permissions}
        loading={isLoading}
        emptyMessage="لا توجد صلاحيات فعالة"
        rowKey={(r) => r.permissionId}
      />

      <Dialog
        open={pendingAction !== null}
        onClose={() => setPendingAction(null)}
        title={`${actionLabel} صلاحية`}
        footer={
          <>
            <Button variant="outline" size="sm" onClick={() => setPendingAction(null)}>
              إلغاء
            </Button>
            <Button variant="primary" size="sm" onClick={confirmAction} disabled={!reason.trim()}>
              تأكيد
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <Input
            label="السبب *"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="أدخل سبب التغيير..."
          />
        </div>
      </Dialog>
    </div>
  );
}
