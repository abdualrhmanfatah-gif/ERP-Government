import { useNavigate } from 'react-router-dom';
import { notify } from '@/features/notifications/notify';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { RoleForm } from '../components/RoleForm';
import { useCreateRole } from '../hooks/useCreateRole';
import type { CreateRoleCommand } from '../types';

export function RoleCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateRole();

  const handleSubmit = async (data: CreateRoleCommand) => {
    try {
      await createMutation.mutateAsync(data);
      notify({ type: 'success', title: 'تم إنشاء الدور بنجاح' });
      navigate('/security/roles');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء الدور';
      notify({ type: 'error', title: message });
    }
  };

  return (
    <div>
      <PageHeader
        title="دور جديد"
        description="إنشاء دور أمان جديد"
        actions={
          <Button variant="ghost" onClick={() => navigate('/security/roles')}>
            إلغاء
          </Button>
        }
      />
      <div className="bg-[var(--color-surface-container-low)] rounded-lg p-6 max-w-2xl">
        <RoleForm
          onSubmit={handleSubmit}
          serverError={createMutation.error ? 'حدث خطأ أثناء إنشاء الدور' : undefined}
          loading={createMutation.isPending}
        />
      </div>
    </div>
  );
}
