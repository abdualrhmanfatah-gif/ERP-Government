import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { RoleForm } from '../components/RoleForm';
import { useRoles } from '../hooks';
import { useUpdateRole } from '../hooks/useUpdateRole';
import type { CreateRoleCommand, UpdateRoleCommand } from '../types';

export function RoleEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const roleId = parseInt(id ?? '0', 10);
  const { data: roles = [], isLoading } = useRoles();
  const updateMutation = useUpdateRole();

  const role = roles.find((r) => r.id === roleId);

  if (isLoading) {
    return <div className="p-8 text-center text-[var(--color-on-surface-variant)]">جاري التحميل...</div>;
  }

  if (!role) {
    return <div className="p-8 text-center text-[var(--color-on-surface-variant)]">الدور غير موجود</div>;
  }

  const handleSubmit = async (data: CreateRoleCommand) => {
    try {
      await updateMutation.mutateAsync({ id: roleId, ...data } as UpdateRoleCommand);
      toast.success('تم تحديث الدور بنجاح');
      navigate('/security/roles');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تحديث الدور';
      toast.error(message);
    }
  };

  return (
    <div>
      <PageHeader
        title={`تعديل الدور: ${role.name}`}
        description="تعديل بيانات الدور"
        actions={
          <Button variant="ghost" onClick={() => navigate('/security/roles')}>
            إلغاء
          </Button>
        }
      />
      <div className="bg-[var(--color-surface-container-low)] rounded-lg p-6 max-w-2xl">
        <RoleForm
          initialData={role}
          isEdit
          onSubmit={handleSubmit}
          serverError={updateMutation.error ? 'حدث خطأ أثناء تحديث الدور' : undefined}
          loading={updateMutation.isPending}
        />
      </div>
    </div>
  );
}
