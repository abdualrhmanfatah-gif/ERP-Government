import { Plus, Pencil, Trash2 } from 'lucide-react';
import { Page, DataGrid, Button, Dialog } from '@/components/ui';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { CostCenterForm } from '@/components/OrganizationCostCenterForm';
import { useCostCenters, useCostCenter, useCreateCostCenter, useUpdateCostCenter, useDeleteCostCenter } from '../hooks';
import { useState } from 'react';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import { notify } from '@/features/notifications/notify';
import type { CreateCostCenterCommand } from '../types';

export function CostCentersListPage() {
  const { data: costCenters = [], isLoading, error, refetch } = useCostCenters();
  const createMutation = useCreateCostCenter();
  const updateMutation = useUpdateCostCenter();
  const deleteMutation = useDeleteCostCenter();
  
  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  
  const { data: editingCostCenter, isLoading: isLoadingEdit } = useCostCenter(editingId ?? 0);

  const handleCreate = async (data: CreateCostCenterCommand) => {
    try {
      await createMutation.mutateAsync(data);
      setDialogOpen(false);
      notify({ type: 'success', title: 'تم إنشاء مركز التكلفة بنجاح' });
    } catch {
      // Error shown via form
    }
  };

  const handleUpdate = async (data: CreateCostCenterCommand) => {
    if (!editingId) return;
    try {
      await updateMutation.mutateAsync({ 
        ...data, 
        id: editingId, 
        isActive: editingCostCenter?.isActive ?? true 
      });
      setDialogOpen(false);
      setEditingId(null);
      notify({ type: 'success', title: 'تم تحديث مركز التكلفة بنجاح' });
    } catch {
      // Error shown via form
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteMutation.mutateAsync(deleteTarget.id);
      setDeleteTarget(null);
      notify({ type: 'success', title: 'تم حذف مركز التكلفة بنجاح' });
    } catch {
      // Error shown via toast
    }
  };

  const openCreateDialog = () => {
    setEditingId(null);
    setDialogOpen(true);
  };

  const openEditDialog = (id: number) => {
    setEditingId(id);
    setDialogOpen(true);
  };

  return (
    <Page
      title="إدارة مراكز التكلفة"
      description="إضافة وتعديل وحذف مراكز التكلفة"
      actions={
        <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={openCreateDialog}>
          مركز تكلفة جديد
        </Button>
      }
    >
      <DataGrid
        columns={[
          { key: 'code', header: 'الكود', width: 120, render: (r) => <span dir="ltr">{r.code}</span> },
          { key: 'name', header: 'الاسم', width: 200 },
          { key: 'organizationUnitName', header: 'الوحدة', width: 150, render: (r) => r.organizationUnitName ?? '—' },
          { key: 'budgetLimit', header: 'حد الميزانية', width: 130, render: (r) => (
            <span className="tabular-nums" dir="ltr">
              {r.budgetLimit != null ? r.budgetLimit.toLocaleString('ar-SA', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '—'}
            </span>
          )},
          { key: 'isActive', header: 'الحالة', width: 100, render: (r) => (
            <StatusBadge variant={r.isActive ? 'active' : 'draft'}>
              {getActiveStatusLabel(r.isActive)}
            </StatusBadge>
          )},
          { key: 'actions', header: 'الإجراءات', width: 100, render: (r) => (
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="icon-xs" onClick={() => openEditDialog(r.id)} title="تعديل">
                <Pencil size={14} />
              </Button>
              <Button variant="ghost" size="icon-xs" onClick={() => setDeleteTarget({ id: r.id, name: r.name })} title="حذف">
                <Trash2 size={14} className="text-[var(--color-error)]" />
              </Button>
            </div>
          )},
        ]}
        data={costCenters}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        rowKey={(r) => String(r.id)}
        emptyMessage="لا توجد مراكز تكلفة"
        onRowClick={(r) => openEditDialog(r.id)}
      />
      
      <Dialog
        open={dialogOpen}
        onClose={() => {
          setDialogOpen(false);
          setEditingId(null);
        }}
        title={editingId ? 'تعديل مركز التكلفة' : 'إضافة مركز تكلفة جديد'}
        footer={
          <>
            <Button variant="ghost" onClick={() => { setDialogOpen(false); setEditingId(null); }} disabled={createMutation.isPending || updateMutation.isPending || isLoadingEdit}>
              إلغاء
            </Button>
            <Button type="submit" variant="primary" loading={createMutation.isPending || updateMutation.isPending || isLoadingEdit} form="cost-center-form">
              {editingId ? 'تحديث' : 'إنشاء'}
            </Button>
          </>
        }
      >
        <CostCenterForm
          id="cost-center-form"
          initialData={editingId ? editingCostCenter : undefined}
          isEdit={!!editingId}
          onSubmit={editingId ? handleUpdate : handleCreate}
          serverError={createMutation.error?.message || updateMutation.error?.message}
          loading={createMutation.isPending || updateMutation.isPending || isLoadingEdit}
        />
      </Dialog>
      
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="حذف مركز التكلفة"
        message={`هل تريد حذف "${deleteTarget?.name}"؟ هذا الإجراء لا يمكن التراجع عنه.`}
        confirmLabel="حذف"
        loading={deleteMutation.isPending}
      />
    </Page>
  );
}
