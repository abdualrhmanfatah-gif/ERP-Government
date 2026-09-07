import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Plus, Trash2 } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Loading } from '@/components/ui/Loading';
import { Card, Input, EmptyState } from '@/components/ui';
import { useRoles, useRolePermissions, useAssignRolePermission, useRemoveRolePermission, usePermissions } from '../hooks';

interface AssignedPermission {
  permissionId: number;
  permissionCode: string;
  permissionName: string;
}

const permColumns: DataGridColumn<AssignedPermission>[] = [
  { header: 'الكود', cell: (row) => <span dir="ltr">{row.permissionCode}</span> },
  { header: 'الاسم', cell: (row) => row.permissionName },
];

export function RoleDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const roleId = parseInt(id ?? '0', 10);
  const { data: roles = [], isLoading } = useRoles();
  const { data: assignedPermissions = [], isLoading: permsLoading } = useRolePermissions(roleId);
  const { data: allPermissions = [] } = usePermissions();
  const assignMutation = useAssignRolePermission();
  const removeMutation = useRemoveRolePermission();

  const [showAdd, setShowAdd] = useState(false);
  const [filter, setFilter] = useState('');

  const role = roles.find((r) => r.id === roleId);

  const availablePermissions = allPermissions.filter(
    (p) => !assignedPermissions.some((ap) => ap.permissionId === p.id) &&
      (filter === '' || p.code.toLowerCase().includes(filter.toLowerCase()) || p.name.toLowerCase().includes(filter.toLowerCase()))
  );

  async function handleAssign(permissionId: number) {
    try {
      await assignMutation.mutateAsync({ roleId, permissionId });
      notify({ type: 'success', title: 'تم تعيين الصلاحية بنجاح' });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تعيين الصلاحية';
      notify({ type: 'error', title: message });
    }
  }

  async function handleRemove(permissionId: number) {
    try {
      await removeMutation.mutateAsync({ roleId, permissionId });
      notify({ type: 'success', title: 'تم إزالة الصلاحية بنجاح' });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إزالة الصلاحية';
      notify({ type: 'error', title: message });
    }
  }

  if (isLoading) {
    return <Loading />;
  }

  if (!role) {
    return <EmptyState message="الدور غير موجود" />;
  }

  const columns: DataGridColumn<AssignedPermission>[] = [
    ...permColumns,
    {
      header: 'إجراء',
      cell: (row) => (
        <Button
          variant="ghost"
          size="icon-xs"
          onClick={() => handleRemove(row.permissionId)}
          disabled={removeMutation.isPending}
          title="إزالة"
        >
          <Trash2 size={14} className="text-[var(--color-error)]" />
        </Button>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title={role.name}
        description={role.description || `كود: ${role.code}`}
        actions={
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate(`/security/roles/${roleId}/edit`)}>
              تعديل
            </Button>
            <Button variant="ghost" onClick={() => navigate('/security/roles')}>
              رجوع
            </Button>
          </div>
        }
      />

      <div className="space-y-6">
        <Card>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الكود:</span>
              <span className="me-2 text-[var(--color-on-surface)] font-medium" dir="ltr">{role.code}</span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">المستوى:</span>
              <span className="me-2 text-[var(--color-on-surface)] font-medium">{role.roleLevel}</span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الحالة:</span>
              <span className="me-2">
                <StatusBadge variant={role.isActive ? 'active' : 'draft'}>
                  {role.isActive ? 'نشط' : 'غير نشط'}
                </StatusBadge>
              </span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">مصادقة ثنائية:</span>
              <span className="me-2 text-[var(--color-on-surface)] font-medium">{role.requiresMfa ? 'نعم' : 'لا'}</span>
            </div>
          </div>
        </Card>

        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-medium text-[var(--color-on-surface)]">الصلاحيات المعيّنة ({assignedPermissions.length})</h3>
            <Button variant="primary" size="sm" icon={<Plus size={14} />} onClick={() => setShowAdd(!showAdd)}>
              إضافة صلاحية
            </Button>
          </div>

          {showAdd && (
            <Card className="p-3">
              <label htmlFor="permission-search" className="sr-only">بحث في الصلاحيات</label>
              <Input
                id="permission-search"
                placeholder="بحث في الصلاحيات..."
                value={filter}
                onChange={(e) => setFilter(e.target.value)}
                className="mb-2"
              />
              <div className="max-h-48 overflow-y-auto space-y-1">
                {availablePermissions.length === 0 && (
                  <div className="text-[var(--color-on-surface-variant)] text-xs p-2">لا توجد صلاحيات متاحة</div>
                )}
                {availablePermissions.map((p) => (
                  <Button
                    key={p.id}
                    variant="ghost"
                    size="sm"
                    onClick={() => handleAssign(p.id)}
                    disabled={assignMutation.isPending}
                    className="w-full text-start justify-between"
                  >
                    <span className="text-[var(--color-on-surface)]">{p.name}</span>
                    <span className="text-[var(--color-on-surface-variant)] text-xs" dir="ltr">{p.code}</span>
                  </Button>
                ))}
              </div>
            </Card>
          )}

          <DataGrid
            columns={columns}
            data={assignedPermissions}
            loading={permsLoading}
            emptyMessage="لا توجد صلاحيات معيّنة"
            rowKey={(row) => row.permissionId}
          />
        </div>
      </div>
    </div>
  );
}
