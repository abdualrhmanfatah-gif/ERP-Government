import { useNavigate } from 'react-router-dom';
import { notify } from '@/features/notifications/notify';
import { Page, Button, Card } from '@/components/ui';
import { RoleForm } from '@/components/SecurityRbacRoleForm';
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
    <Page
      title="دور جديد"
      description="إنشاء دور أمان جديد"
      maxWidth="sm"
      actions={
        <Button variant="ghost" onClick={() => navigate('/security/roles')}>
          إلغاء
        </Button>
      }
    >
      <Card>
        <RoleForm
          onSubmit={handleSubmit}
          serverError={createMutation.error ? 'حدث خطأ أثناء إنشاء الدور' : undefined}
          loading={createMutation.isPending}
        />
      </Card>
    </Page>
  );
}
