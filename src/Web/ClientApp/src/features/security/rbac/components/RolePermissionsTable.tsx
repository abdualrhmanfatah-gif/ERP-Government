import { useState } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/Button';
import { useRolePermissions, useAssignRolePermission, useRemoveRolePermission, usePermissions } from '../hooks';

interface RolePermissionsTableProps {
  roleId: number;
}

export function RolePermissionsTable({ roleId }: RolePermissionsTableProps) {
  const { data: assignedPermissions = [], isLoading } = useRolePermissions(roleId);
  const { data: allPermissions = [] } = usePermissions();
  const assignMutation = useAssignRolePermission();
  const removeMutation = useRemoveRolePermission();

  const [showAdd, setShowAdd] = useState(false);
  const [filter, setFilter] = useState('');

  const availablePermissions = allPermissions.filter(
    (p) => !assignedPermissions.some((ap) => ap.permissionId === p.id) &&
      (filter === '' || p.code.toLowerCase().includes(filter.toLowerCase()) || p.name.toLowerCase().includes(filter.toLowerCase()))
  );

  const handleAssign = async (permissionId: number) => {
    try {
      await assignMutation.mutateAsync({ roleId, permissionId });
      toast.success('تم تعيين الصلاحية بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تعيين الصلاحية';
      toast.error(message);
    }
  };

  const handleRemove = async (permissionId: number) => {
    try {
      await removeMutation.mutateAsync({ roleId, permissionId });
      toast.success('تم إزالة الصلاحية بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إزالة الصلاحية';
      toast.error(message);
    }
  };

  if (isLoading) {
    return <div className="text-[var(--color-on-surface-variant)] text-sm p-4">جاري التحميل...</div>;
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-[var(--color-on-surface)]">الصلاحيات المعيّنة ({assignedPermissions.length})</h3>
        <Button variant="primary" size="sm" icon={<Plus size={14} />} onClick={() => setShowAdd(!showAdd)}>
          إضافة صلاحية
        </Button>
      </div>

      {showAdd && (
        <div className="p-3 bg-[var(--color-surface-container)] rounded-lg border border-[var(--color-border-container)]">
          <label htmlFor="permission-search" className="sr-only">بحث في الصلاحيات</label>
          <input
            id="permission-search"
            type="text"
            placeholder="بحث في الصلاحيات..."
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            className="w-full px-3 py-2 mb-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface)] text-[var(--color-on-surface)] text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
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
        </div>
      )}

      <div className="border border-[var(--color-border-container)] rounded">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-[var(--color-surface-container)] border-b border-[var(--color-border-container)]">
              <th className="px-3 py-2 text-start text-[var(--color-on-surface)] font-medium">الكود</th>
              <th className="px-3 py-2 text-start text-[var(--color-on-surface)] font-medium">الاسم</th>
              <th className="px-3 py-2 text-start text-[var(--color-on-surface)] font-medium w-20">إجراء</th>
            </tr>
          </thead>
          <tbody>
            {assignedPermissions.length === 0 && (
              <tr>
                <td colSpan={3} className="px-3 py-4 text-center text-[var(--color-on-surface-variant)]">لا توجد صلاحيات معيّنة</td>
              </tr>
            )}
            {assignedPermissions.map((rp) => (
              <tr key={rp.permissionId} className="border-b border-[var(--color-border-container)] last:border-0 hover:bg-[var(--color-surface-container-low)]">
                <td className="px-3 py-2 text-[var(--color-on-surface)]" dir="ltr">{rp.permissionCode}</td>
                <td className="px-3 py-2 text-[var(--color-on-surface)]">{rp.permissionName}</td>
                <td className="px-3 py-2">
                  <Button
                    variant="ghost"
                    size="icon-xs"
                    onClick={() => handleRemove(rp.permissionId)}
                    disabled={removeMutation.isPending}
                    title="إزالة"
                  >
                    <Trash2 size={14} className="text-[var(--color-error)]" />
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
