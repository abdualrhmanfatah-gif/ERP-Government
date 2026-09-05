import { Plus, Pencil, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { Button } from '@/components/ui/Button';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { useOrganizationalUnits, useDeleteOrgUnit } from '../hooks';
import { useState } from 'react';

export function OrgUnitsListPage() {
  const navigate = useNavigate();
  const { data: orgUnits = [], isLoading, error, refetch } = useOrganizationalUnits();
  const deleteMutation = useDeleteOrgUnit();
  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteMutation.mutateAsync(deleteTarget.id);
      setDeleteTarget(null);
    } catch {
      // Error shown via toast in parent
    }
  };

  return (
    <div>
      <PageHeader
        title="إدارة الوحدات التنظيمية"
        description="إضافة وتعديل وحذف الوحدات التنظيمية"
        actions={
          <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/organization/units/create')}>
            وحدة جديدة
          </Button>
        }
      />
      <DataGrid
        columns={[
          { key: 'code', header: 'الكود', width: 120, render: (r) => <span dir="ltr">{r.code}</span> },
          { key: 'name', header: 'الاسم', width: 200 },
          { key: 'parentName', header: 'الأب', width: 150, render: (r) => r.parentName ?? '—' },
          { key: 'isActive', header: 'الحالة', width: 100, render: (r) => (
            <StatusBadge variant={r.isActive ? 'active' : 'draft'}>
              {r.isActive ? 'نشط' : 'غير نشط'}
            </StatusBadge>
          )},
          { key: 'actions', header: 'الإجراءات', width: 100, render: (r) => (
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="icon-xs" onClick={() => navigate(`/organization/units/${r.id}/edit`)} title="تعديل">
                <Pencil size={14} />
              </Button>
              <Button variant="ghost" size="icon-xs" onClick={() => setDeleteTarget({ id: r.id, name: r.name })} title="حذف">
                <Trash2 size={14} className="text-[var(--color-error)]" />
              </Button>
            </div>
          )},
        ]}
        data={orgUnits}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        rowKey={(r) => String(r.id)}
        emptyMessage="لا توجد وحدات تنظيمية"
        onRowClick={(r) => navigate(`/organization/units/${r.id}/edit`)}
      />
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="حذف الوحدة التنظيمية"
        message={`هل تريد حذف "${deleteTarget?.name}"؟ هذا الإجراء لا يمكن التراجع عنه.`}
        confirmLabel="حذف"
        loading={deleteMutation.isPending}
      />
    </div>
  );
}
