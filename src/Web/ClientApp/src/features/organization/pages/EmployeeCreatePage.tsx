import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { Loading, EmptyState } from '@/components/ui';
import { EmployeeForm } from '@/components/OrganizationEmployeeForm';
import { useEmployee, useCreateEmployee, useUpdateEmployee } from '../hooks';
import type { CreateEmployeeCommand, UpdateEmployeeCommand } from '../types';

export function EmployeeCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateEmployee();

  const handleSubmit = async (data: CreateEmployeeCommand) => {
    await createMutation.mutateAsync(data);
    navigate('/organization/employees');
  };

  return (
    <div>
      <PageHeader title="موظف جديد" description="إضافة موظف جديد" />
      <div className="max-w-xl">
        <EmployeeForm
          onSubmit={handleSubmit}
          serverError={createMutation.error?.message}
          loading={createMutation.isPending}
        />
      </div>
    </div>
  );
}

export function EmployeeEditPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const employeeId = parseInt(id ?? '0', 10);
  const { data: employee, isLoading } = useEmployee(employeeId);
  const updateMutation = useUpdateEmployee();

  const handleSubmit = async (data: CreateEmployeeCommand | UpdateEmployeeCommand) => {
    await updateMutation.mutateAsync({ ...data, id: employeeId, isActive: employee?.isActive ?? true } as UpdateEmployeeCommand);
    navigate('/organization/employees');
  };

  if (isLoading) return <Loading />;
  if (!employee) return <EmptyState message="الموظف غير موجود" />;

  return (
    <div>
      <PageHeader title={`تعديل: ${employee.name}`} description="تحديث بيانات الموظف" />
      <div className="max-w-xl">
        <EmployeeForm
          initialData={employee}
          isEdit
          onSubmit={handleSubmit}
          serverError={updateMutation.error?.message}
          loading={updateMutation.isPending}
        />
      </div>
    </div>
  );
}
