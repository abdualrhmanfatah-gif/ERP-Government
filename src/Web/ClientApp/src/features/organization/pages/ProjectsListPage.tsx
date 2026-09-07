import { Plus, Pencil, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Page, DataGrid, Button } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { useProjects, useDeleteProject } from '../hooks';
import { useState } from 'react';

const statusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Active: 'نشط',
  OnHold: 'معلق',
  Completed: 'مكتمل',
  Cancelled: 'ملغي',
};

export function ProjectsListPage() {
  const navigate = useNavigate();
  const { data: projects = [], isLoading, error, refetch } = useProjects();
  const deleteMutation = useDeleteProject();
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
      title="إدارة المشاريع"
      description="إضافة وتعديل وحذف المشاريع"
      actions={
        <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/organization/projects/create')}>
          مشروع جديد
        </Button>
      }
    >
      <DataGrid
        columns={[
          { key: 'code', header: 'الكود', width: 120, render: (r) => <span dir="ltr">{r.code}</span> },
          { key: 'name', header: 'الاسم', width: 200 },
          { key: 'costCenterName', header: 'مركز التكلفة', width: 150, render: (r) => r.costCenterName ?? '—' },
          { key: 'status', header: 'الحالة', width: 100, render: (r) => {
            const variantMap: Record<string, 'draft' | 'pending' | 'approved' | 'active' | 'closed'> = {
              Draft: 'draft',
              Active: 'active',
              OnHold: 'pending',
              Completed: 'approved',
              Cancelled: 'closed',
            };
            return (
              <StatusBadge variant={variantMap[r.status] ?? 'draft'}>
                {statusLabels[r.status] ?? r.status}
              </StatusBadge>
            );
          }},
          { key: 'budgetAmount', header: 'المبلغ المخصص', width: 130, render: (r) => (
            <span className="tabular-nums" dir="ltr">
              {r.budgetAmount != null ? r.budgetAmount.toLocaleString('ar-SA', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '—'}
            </span>
          )},
          { key: 'actions', header: 'الإجراءات', width: 100, render: (r) => (
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="icon-xs" onClick={() => navigate(`/organization/projects/${r.id}/edit`)} title="تعديل">
                <Pencil size={14} />
              </Button>
              <Button variant="ghost" size="icon-xs" onClick={() => setDeleteTarget({ id: r.id, name: r.name })} title="حذف">
                <Trash2 size={14} className="text-[var(--color-error)]" />
              </Button>
            </div>
          )},
        ]}
        data={projects}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        rowKey={(r) => String(r.id)}
        emptyMessage="لا توجد مشاريع"
        onRowClick={(r) => navigate(`/organization/projects/${r.id}/edit`)}
      />
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="حذف المشروع"
        message={`هل تريد حذف "${deleteTarget?.name}"؟ هذا الإجراء لا يمكن التراجع عنه.`}
        confirmLabel="حذف"
        loading={deleteMutation.isPending}
      />
    </Page>
  );
}
