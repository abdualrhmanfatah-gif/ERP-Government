import { Plus, Pencil, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Page, DataGrid, Button } from '@/components/ui';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { useEmployees, useDeleteEmployee } from '../hooks';
import { employeeStatusLabels } from '../types';
import { useState } from 'react';

export function EmployeesListPage() {
  const navigate = useNavigate();
  const { data: employees = [], isLoading, error, refetch } = useEmployees();
  const deleteMutation = useDeleteEmployee();
  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteMutation.mutateAsync(deleteTarget.id);
      setDeleteTarget(null);
    } catch {
      // Error shown via toast
    }
  };

  return (
    <Page
      title="إدارة الموظفين"
      description="إضافة وتعديل وحذف الموظفين"
      actions={
        <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/organization/employees/create')}>
          موظف جديد
        </Button>
      }
    >
      <DataGrid
        columns={[
          { key: 'employeeNumber', header: 'رقم الموظف', width: 130, render: (r) => <span dir="ltr">{r.employeeNumber}</span> },
          { key: 'name', header: 'الاسم', width: 180, render: (r) => r.name },
          { key: 'organizationalUnitName', header: 'الوحدة', width: 150 },
          { key: 'jobTitle', header: 'المسمى الوظيفي', width: 150 },
          { key: 'employmentStatus', header: 'الحالة', width: 100, render: (r) => (
            <StatusBadge variant={r.isActive ? 'active' : 'draft'}>
              {employeeStatusLabels[r.employmentStatus] ?? r.employmentStatus}
            </StatusBadge>
          )},
          { key: 'actions', header: 'الإجراءات', width: 100, render: (r) => (
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="icon-xs" onClick={() => navigate(`/organization/employees/${r.id}/edit`)} title="تعديل">
                <Pencil size={14} />
              </Button>
              <Button variant="ghost" size="icon-xs" onClick={() => setDeleteTarget({ id: r.id, name: r.name })} title="حذف">
                <Trash2 size={14} className="text-[var(--color-error)]" />
              </Button>
            </div>
          )},
        ]}
        data={employees}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        rowKey={(r) => String(r.id)}
        emptyMessage="لا يوجد موظفون"
        onRowClick={(r) => navigate(`/organization/employees/${r.id}/edit`)}
      />
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="حذف الموظف"
        message={`هل تريد حذف "${deleteTarget?.name}"؟ هذا الإجراء لا يمكن التراجع عنه.`}
        confirmLabel="حذف"
        loading={deleteMutation.isPending}
      />
    </Page>
  );
}
