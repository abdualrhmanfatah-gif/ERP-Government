import { useState } from 'react';
import { toast } from 'sonner';
import { Shield, ShieldOff, Save } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { DataGrid } from '@/components/ui/DataGrid';
import { Input } from '@/components/ui/Input';
import { FormField } from '@/components/ui/FormField';
import { useUserDetail, useSetUserRole, useUserPermissions, useAssignUserPermission } from '../hooks';
import { rolesClient } from '../../rbac/client';
import { useQuery } from '@tanstack/react-query';
import type { EffectivePermissionDto } from '../types';

interface RolesTabProps {
  userId: number;
}

export function RolesTab({ userId }: RolesTabProps) {
  const { data: user } = useUserDetail(userId);
  const setRole = useSetUserRole();
  const { data: effectivePermissions = [] } = useUserPermissions(userId);
  const assignPermission = useAssignUserPermission();
  const [selectedRoleId, setSelectedRoleId] = useState('');
  const [overrideReason, setOverrideReason] = useState('');
  const [pendingOverrides, setPendingOverrides] = useState<Map<number, boolean>>(new Map());

  const { data: allRoles = [] } = useQuery({
    queryKey: ['rbac-roles'],
    queryFn: () => rolesClient.list(),
  });

  const currentRoleId = user?.role?.roleId;
  const availableRoles = allRoles.filter((r) => r.isActive);

  const inheritedPermissions = effectivePermissions.filter((p) => p.source === 'role');
  const overridePermissions = effectivePermissions.filter((p) => p.source === 'override');

  function handleRevoke(perm: EffectivePermissionDto) {
    setPendingOverrides((prev) => {
      const next = new Map(prev);
      if (next.has(perm.permissionId)) {
        next.delete(perm.permissionId);
      } else {
        next.set(perm.permissionId, false);
      }
      return next;
    });
  }

  function handleGrant(perm: EffectivePermissionDto) {
    setPendingOverrides((prev) => {
      const next = new Map(prev);
      if (next.has(perm.permissionId)) {
        next.delete(perm.permissionId);
      } else {
        next.set(perm.permissionId, true);
      }
      return next;
    });
  }

  async function handleSetRole() {
    if (!selectedRoleId) return;
    try {
      await setRole.mutateAsync({ userId, data: { userId, roleId: Number(selectedRoleId) } });
      toast.success('تم تعيين الدور بنجاح');
      setSelectedRoleId('');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'حدث خطأ';
      toast.error(message);
    }
  }

  async function handleSaveOverrides() {
    if (!overrideReason.trim()) {
      toast.error('يجب إدخال سبب التغيير');
      return;
    }
    try {
      for (const [permId, isGranted] of pendingOverrides) {
        await assignPermission.mutateAsync({
          userId,
          data: { userId, permissionId: permId, isGranted, reason: overrideReason },
        });
      }
      toast.success('تم حفظ التغييرات بنجاح');
      setPendingOverrides(new Map());
      setOverrideReason('');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'حدث خطأ';
      toast.error(message);
    }
  }

  const inheritedColumns = [
    { key: 'code', header: 'الكود', accessorKey: 'code' as const, width: 140 },
    { key: 'name', header: 'الاسم', accessorKey: 'name' as const, width: 200 },
    {
      key: 'status',
      header: 'الحالة',
      width: 100,
      render: (_p: EffectivePermissionDto) => (
        <span className="text-xs px-2 py-0.5 rounded-lg bg-[var(--color-primary-container)] text-[var(--color-on-primary-container)]">
          موروث
        </span>
      ),
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      width: 80,
      render: (p: EffectivePermissionDto) => {
        const override = pendingOverrides.get(p.permissionId);
        return (
          <Button
            variant={override === false ? 'error' : 'ghost'}
            size="icon-xs"
            onClick={() => handleRevoke(p)}
            title="حجب"
          >
            <ShieldOff size={14} className={override === false ? 'text-white' : 'text-[var(--color-error)]'} />
          </Button>
        );
      },
    },
  ];

  const overrideColumns = [
    { key: 'code', header: 'الكود', accessorKey: 'code' as const, width: 140 },
    { key: 'name', header: 'الاسم', accessorKey: 'name' as const, width: 200 },
    {
      key: 'status',
      header: 'الحالة',
      width: 100,
      render: (p: EffectivePermissionDto) => {
        const pending = pendingOverrides.get(p.permissionId);
        const granted = pending ?? p.isGranted;
        return (
          <span className={`text-xs px-2 py-0.5 rounded-lg ${granted ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
            {granted ? 'ممنوح' : 'محجوب'}
          </span>
        );
      },
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      width: 80,
      render: (p: EffectivePermissionDto) => {
        const override = pendingOverrides.get(p.permissionId);
        return (
          <Button
            variant={override === true ? 'success' : 'ghost'}
            size="icon-xs"
            onClick={() => handleGrant(p)}
            title="منح"
          >
            <Shield size={14} className={override === true ? 'text-white' : 'text-[var(--color-primary)]'} />
          </Button>
        );
      },
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex gap-3 items-end">
        <div className="flex-1 max-w-xs">
          <Select
            label="تعيين دور"
            value={selectedRoleId}
            onChange={(e) => setSelectedRoleId(e.target.value)}
            options={[
              { value: '', label: '— اختر دوراً —' },
              ...availableRoles.map((r) => ({ value: String(r.id), label: r.name })),
            ]}
          />
        </div>
        <Button
          variant="primary"
          size="sm"
          onClick={handleSetRole}
          disabled={!selectedRoleId || setRole.isPending}
          className="mb-6"
        >
          تعيين
        </Button>
      </div>

      {currentRoleId && (
        <div className="text-sm text-[var(--color-on-surface-variant)]">
          الدور الحالي: <span className="font-medium">{user?.role?.name ?? '—'}</span>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4">
        <div>
          <h3 className="text-sm font-medium mb-2 text-[var(--color-on-surface)]">الصلاحيات الموروثة من الدور</h3>
          <DataGrid
            columns={inheritedColumns}
            data={inheritedPermissions}
            emptyMessage="لا توجد صلاحيات موروثة"
            rowKey={(p) => p.permissionId}
          />
        </div>
        <div>
          <h3 className="text-sm font-medium mb-2 text-[var(--color-on-surface)]">التجاوزات (User Overrides)</h3>
          <DataGrid
            columns={overrideColumns}
            data={overridePermissions}
            emptyMessage="لا توجد تجاوزات"
            rowKey={(p) => p.permissionId}
          />
        </div>
      </div>

      {pendingOverrides.size > 0 && (
        <div className="bg-[var(--color-surface)] rounded-lg border border-[var(--color-border-container)] p-4 space-y-3">
          <FormField label="سبب التغيير (إلزامي)">
            <Input
              value={overrideReason}
              onChange={(e) => setOverrideReason(e.target.value)}
              placeholder="أدخل سبب التغيير..."
            />
          </FormField>
          <div className="flex gap-2">
            <Button
              variant="primary"
              size="sm"
              icon={<Save size={14} />}
              onClick={handleSaveOverrides}
              disabled={!overrideReason.trim() || assignPermission.isPending}
            >
              حفظ التغييرات ({pendingOverrides.size})
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setPendingOverrides(new Map());
                setOverrideReason('');
              }}
            >
              إلغاء
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
