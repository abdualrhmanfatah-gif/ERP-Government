import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { RolePermissionsTable } from '../components/RolePermissionsTable';
import { useRoles } from '../hooks';

export function RoleDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const roleId = parseInt(id ?? '0', 10);
  const { data: roles = [], isLoading } = useRoles();

  const role = roles.find((r) => r.id === roleId);

  if (isLoading) {
    return <div className="p-8 text-center text-[var(--color-on-surface-variant)]">جاري التحميل...</div>;
  }

  if (!role) {
    return <div className="p-8 text-center text-[var(--color-on-surface-variant)]">الدور غير موجود</div>;
  }

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
        <div className="bg-[var(--color-surface-container-lowest)] rounded-lg p-4">
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
        </div>

        <RolePermissionsTable roleId={roleId} />
      </div>
    </div>
  );
}
