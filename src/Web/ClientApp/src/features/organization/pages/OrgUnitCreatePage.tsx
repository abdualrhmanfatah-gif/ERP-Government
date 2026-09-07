import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { OrgUnitForm } from '@/components/OrganizationOrgUnitForm';
import { useOrganizationalUnit, useCreateOrgUnit, useUpdateOrgUnit } from '../hooks';
import type { CreateOrgUnitCommand } from '../types';

export function OrgUnitCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateOrgUnit();

  const handleSubmit = async (data: CreateOrgUnitCommand) => {
    await createMutation.mutateAsync(data);
    navigate('/organization/units');
  };

  return (
    <div>
      <PageHeader title="وحدة تنظيمية جديدة" description="إضافة وحدة تنظيمية جديدة" />
      <div className="max-w-xl">
        <OrgUnitForm
          onSubmit={handleSubmit}
          serverError={createMutation.error?.message}
          loading={createMutation.isPending}
        />
      </div>
    </div>
  );
}

export function OrgUnitEditPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const orgUnitId = parseInt(id ?? '0', 10);
  const { data: orgUnit, isLoading } = useOrganizationalUnit(orgUnitId);
  const updateMutation = useUpdateOrgUnit();

  const handleSubmit = async (data: CreateOrgUnitCommand) => {
    await updateMutation.mutateAsync({ ...data, id: orgUnitId, isActive: orgUnit?.isActive ?? true });
    navigate('/organization/units');
  };

  if (isLoading) return <div className="p-4">جاري التحميل...</div>;
  if (!orgUnit) return <div className="p-4">الوحدة غير موجودة</div>;

  return (
    <div>
      <PageHeader title={`تعديل: ${orgUnit.name}`} description="تحديث بيانات الوحدة التنظيمية" />
      <div className="max-w-xl">
        <OrgUnitForm
          initialData={orgUnit}
          isEdit
          onSubmit={handleSubmit}
          serverError={updateMutation.error?.message}
          loading={updateMutation.isPending}
        />
      </div>
    </div>
  );
}
