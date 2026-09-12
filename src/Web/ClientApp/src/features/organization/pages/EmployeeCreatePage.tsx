import { useNavigate, useParams } from 'react-router-dom';
import { Page, Loading, EmptyState } from '@/components/ui';
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
    <Page title="موظف جديد" description="إضافة موظف جديد" maxWidth="sm">
      <EmployeeForm
        onSubmit={handleSubmit}
        serverError={createMutation.error?.message}
        loading={createMutation.isPending}
      />
    </Page>
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
    <Page title={`تعديل: ${employee.name}`} description="تحديث بيانات الموظف" maxWidth="sm">
      <EmployeeForm
        initialData={employee}
        isEdit
        onSubmit={handleSubmit}
        serverError={updateMutation.error?.message}
        loading={updateMutation.isPending}
      />
    </Page>
  );
}
