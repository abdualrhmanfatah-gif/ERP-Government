import { useState } from 'react';
import { Plus, Pencil, ToggleLeft, ToggleRight } from 'lucide-react';
import { toast } from 'sonner';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { DataGrid } from '@/components/ui/DataGrid';
import { FilterBar, FilterSelect } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { DocumentSequenceForm } from '../components/DocumentSequenceForm';
import { useDocumentSequences } from '../hooks/useDocumentSequences';
import { useCreateDocumentSequence } from '../hooks/useCreateDocumentSequence';
import { useUpdateDocumentSequence } from '../hooks/useUpdateDocumentSequence';
import { useDeactivateDocumentSequence } from '../hooks/useDeactivateDocumentSequence';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import type { DocumentSequenceDto } from '../types';

const resetPolicyLabel = (p: string) => (p === 'Yearly' ? 'سنوي' : 'أبداً');
const docTypeLabel = (t: string) => t;

export function DocumentSequencesListPage() {
  const [filters, setFilters] = useState<{ isActive?: boolean }>({});
  const { data: sequences = [], isLoading, error, refetch } = useDocumentSequences(filters);

  const { hasPermission: canCreate } = usePermission(PERMISSIONS.DocumentSequences.Create);

  const createMutation = useCreateDocumentSequence();
  const updateMutation = useUpdateDocumentSequence();
  const deactivateMutation = useDeactivateDocumentSequence();

  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [editingItem, setEditingItem] = useState<DocumentSequenceDto | null>(null);
  const [confirmDeactivate, setConfirmDeactivate] = useState<DocumentSequenceDto | null>(null);

  const handleCreate = async (data: Record<string, unknown>) => {
    try {
      await createMutation.mutateAsync(data as never);
      setShowCreateDialog(false);
      toast.success('تم إنشاء التسلسل بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء التسلسل';
      toast.error(message);
    }
  };

  const handleEdit = async (data: Record<string, unknown>) => {
    if (!editingItem) return;
    try {
      await updateMutation.mutateAsync({ id: editingItem.id, ...data } as never);
      setEditingItem(null);
      toast.success('تم تعديل التسلسل بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تعديل التسلسل';
      toast.error(message);
    }
  };

  const handleDeactivate = async () => {
    if (!confirmDeactivate) return;
    try {
      await deactivateMutation.mutateAsync(confirmDeactivate.id);
      setConfirmDeactivate(null);
      toast.success('تم تعطيل التسلسل بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تعطيل التسلسل';
      toast.error(message);
    }
  };

  const hasFilter = filters.isActive !== undefined;

  return (
    <div>
      <PageHeader
        title="تسلسل المستندات"
        description="إدارة أرقام التسلسل للمستندات المالية"
        actions={
          canCreate ? (
            <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => setShowCreateDialog(true)}>
              تسلسل جديد
            </Button>
          ) : undefined
        }
      />
      <FilterBar
        hasFilters={hasFilter}
        onClear={() => setFilters({})}
      >
        <FilterSelect
          label="الحالة"
          value={filters.isActive === true ? 'active' : filters.isActive === false ? 'inactive' : ''}
          onChange={(val) => {
            setFilters(val === 'active' ? { isActive: true } : val === 'inactive' ? { isActive: false } : {});
          }}
          options={[
            { value: 'active', label: 'نشط' },
            { value: 'inactive', label: 'غير نشط' },
          ]}
        />
      </FilterBar>
      <DataGrid
        columns={[
          { key: 'name', header: 'الاسم', width: 200 },
          { key: 'documentType', header: 'نوع المستند', width: 150, render: (r) => docTypeLabel(r.documentType) },
          { key: 'currentNumber', header: 'الرقم الحالي', width: 120, align: 'right', render: (r) => String(r.currentNumber).padStart(6, '0') },
          { key: 'resetPolicy', header: 'سياسة إعادة التعيين', width: 150, render: (r) => resetPolicyLabel(r.resetPolicy) },
          { key: 'isActive', header: 'الحالة', width: 100, cell: (r) => (
            <StatusBadge variant={r.isActive ? 'active' : 'closed'}>
              {r.isActive ? 'نشط' : 'غير نشط'}
            </StatusBadge>
          )},
          { key: 'actions', header: 'الإجراءات', width: 120, render: (r) => (
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="icon-xs" onClick={(e) => { e.stopPropagation(); setEditingItem(r); }} title="تعديل">
                <Pencil size={14} />
              </Button>
              <Button variant="ghost" size="icon-xs" onClick={(e) => { e.stopPropagation(); setConfirmDeactivate(r); }} title={r.isActive ? 'تعطيل' : 'تفعيل'}>
                {r.isActive ? <ToggleRight size={14} className="text-[var(--color-success)]" /> : <ToggleLeft size={14} className="text-[var(--color-on-surface-variant)]" />}
              </Button>
            </div>
          )},
        ]}
        data={sequences}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        rowKey={(r) => String(r.id)}
        emptyMessage="لا توجد تسلسلات"
      />

      <Dialog open={showCreateDialog} onClose={() => setShowCreateDialog(false)} title="تسلسل جديد">
        <DocumentSequenceForm
          onSubmit={handleCreate}
          serverError={createMutation.error ? 'حدث خطأ أثناء إنشاء التسلسل' : undefined}
          loading={createMutation.isPending}
        />
      </Dialog>

      <Dialog open={!!editingItem} onClose={() => setEditingItem(null)} title="تعديل التسلسل">
        {editingItem ? (
          <DocumentSequenceForm
            initialData={editingItem}
            isEdit
            onSubmit={handleEdit}
            serverError={updateMutation.error ? 'حدث خطأ أثناء تعديل التسلسل' : undefined}
            loading={updateMutation.isPending}
          />
        ) : null}
      </Dialog>

      <ConfirmDialog
        open={!!confirmDeactivate}
        onClose={() => setConfirmDeactivate(null)}
        onConfirm={handleDeactivate}
        message={`هل تريد تعطيل تسلسل "${confirmDeactivate?.name}"؟`}
        destructive
      />
    </div>
  );
}
