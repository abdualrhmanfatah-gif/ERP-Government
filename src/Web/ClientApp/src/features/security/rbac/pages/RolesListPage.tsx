import { Plus, Pencil, Eye, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { useRoles } from '../hooks';

export function RolesListPage() {
  const navigate = useNavigate();
  const { data: roles = [], isLoading, error, refetch } = useRoles();

  return (
    <div>
      <PageHeader
        title="إدارة الأدوار"
        description="إدارة أدوار الأمان والصلاحيات"
        actions={
          <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/security/roles/create')}>
            دور جديد
          </Button>
        }
      />
      <DataGrid
        columns={[
          { key: 'code', header: 'الكود', width: 150, render: (r) => <span dir="ltr">{r.code}</span> },
          { key: 'name', header: 'الاسم', width: 200 },
          { key: 'roleLevel', header: 'المستوى', width: 120 },
          { key: 'isActive', header: 'الحالة', width: 100, render: (r) => (
            <StatusBadge variant={r.isActive ? 'active' : 'draft'}>
              {r.isActive ? 'نشط' : 'غير نشط'}
            </StatusBadge>
          )},
          { key: 'actions', header: 'الإجراءات', width: 120, render: (r) => (
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="icon-xs" onClick={() => navigate(`/security/roles/${r.id}`)} title="عرض">
                <Eye size={14} />
              </Button>
              <Button variant="ghost" size="icon-xs" onClick={() => navigate(`/security/roles/${r.id}/edit`)} title="تعديل">
                <Pencil size={14} />
              </Button>
              <Button variant="ghost" size="icon-xs" onClick={() => { if (confirm(`هل تريد حذف الدور "${r.name}"؟`)) { /* TODO */ } }} title="حذف">
                <Trash2 size={14} className="text-[var(--color-error)]" />
              </Button>
            </div>
          )},
        ]}
        data={roles}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        rowKey={(r) => String(r.id)}
        emptyMessage="لا توجد أدوار"
        onRowClick={(r) => navigate(`/security/roles/${r.id}`)}
      />
    </div>
  );
}
