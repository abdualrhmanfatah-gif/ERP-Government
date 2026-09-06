import { useMemo, useState } from 'react';
import { usePermission } from '@/shared/hooks/usePermission';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { Button, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Pencil } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { budgetControlMethodLabels, BudgetControlMethod } from '../../shared/types';
import type { BudgetTypeDto } from '../../shared/types';
import { useBudgetTypesList, useCreateBudgetType, useUpdateBudgetType, useToggleBudgetTypeActive } from '../hooks/useBudgetTypes';

const controlMethodBadgeVariant: Record<BudgetControlMethod, 'success' | 'warning' | 'danger'> = {
  [BudgetControlMethod.None]: 'success',
  [BudgetControlMethod.Warning]: 'warning',
  [BudgetControlMethod.Blocking]: 'danger',
};

const controlMethodOptions = Object.entries(budgetControlMethodLabels).map(([value, label]) => ({
  value,
  label,
}));

const isActiveOptions = [
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'معطل' },
];

const columns: DataGridColumn<BudgetTypeDto>[] = [
  { header: 'الكود', cell: (row) => <span className="font-mono">{row.code}</span> },
  { header: 'الاسم', cell: (row) => row.name },
  { header: 'طريقة التحكم', cell: (row) => <Badge variant={controlMethodBadgeVariant[row.controlMethod]}>{budgetControlMethodLabels[row.controlMethod]}</Badge> },
  { header: 'السماح بالتجاوز', cell: (row) => <Switch checked={row.allowOverrun} disabled /> },
];

export default function BudgetTypesListPage() {
  const canManage = usePermission(BUDGET_PERMISSIONS.BudgetTypes.Create);
  const [search, setSearch] = useState('');
  const [controlMethodFilter, setControlMethodFilter] = useState<string>('');
  const [isActiveFilter, setIsActiveFilter] = useState<string>('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<BudgetTypeDto | null>(null);
  const [confirmToggle, setConfirmToggle] = useState<BudgetTypeDto | null>(null);

  const { data: items = [], isLoading } = useBudgetTypesList();
  const createMutation = useCreateBudgetType();
  const updateMutation = useUpdateBudgetType();
  const toggleMutation = useToggleBudgetTypeActive();

  const filtered = useMemo(() => {
    return items.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.code.toLowerCase().includes(q) && !item.name.toLowerCase().includes(q)) return false;
      }
      if (controlMethodFilter && Number(controlMethodFilter) !== item.controlMethod) return false;
      if (isActiveFilter && (isActiveFilter === 'true') !== item.isActive) return false;
      return true;
    });
  }, [items, search, controlMethodFilter, isActiveFilter]);

  const hasFilters = !!search || !!controlMethodFilter || !!isActiveFilter;

  function handleClearFilters() {
    setSearch('');
    setControlMethodFilter('');
    setIsActiveFilter('');
  }

  function handleCreate() {
    setEditItem(null);
    setDialogOpen(true);
  }

  function handleEdit(item: BudgetTypeDto) {
    setEditItem(item);
    setDialogOpen(true);
  }

  function handleToggle(item: BudgetTypeDto) {
    setConfirmToggle(item);
  }

  function confirmToggleAction() {
    if (!confirmToggle) return;
    toggleMutation.mutate(
      { id: confirmToggle.id, rowVersion: confirmToggle.rowVersion, isActive: !confirmToggle.isActive },
      {
        onSuccess: () => {
          notify({ type: 'success', title: confirmToggle.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح' });
          setConfirmToggle(null);
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التبديل' }),
      },
    );
  }

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    const data = {
      code: form.get('code') as string,
      name: form.get('name') as string,
      description: (form.get('description') as string) || undefined,
      controlMethod: Number(form.get('controlMethod')) as BudgetControlMethod,
      allowOverrun: form.get('allowOverrun') === 'on',
    };

    if (editItem) {
      updateMutation.mutate(
        { id: editItem.id, rowVersion: editItem.rowVersion, ...data },
        {
          onSuccess: () => {
            notify({ type: 'success', title: 'تم التحديث بنجاح' });
            setDialogOpen(false);
          },
          onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التحديث' }),
        },
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم الإنشاء بنجاح' });
          setDialogOpen(false);
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
      });
    }
  }

  const allColumns: DataGridColumn<BudgetTypeDto>[] = [
    ...columns,
    {
      header: 'الحالة',
      cell: (row) => canManage
        ? <Switch checked={row.isActive} onChange={() => handleToggle(row)} label={row.isActive ? 'نشط' : 'معطل'} />
        : <Badge variant={row.isActive ? 'success' : 'danger'}>{row.isActive ? 'نشط' : 'معطل'}</Badge>,
    },
    ...(canManage ? [{
      header: 'إجراءات',
      cell: (row: BudgetTypeDto) => <Button variant="ghost" size="icon" onClick={() => handleEdit(row)} aria-label="تعديل"><Pencil size={16} /></Button>,
    }] : []),
  ];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">أنواع الميزانيات</h1>
        {canManage && (
          <Button onClick={handleCreate} icon={<Plus size={16} />}>
            إضافة نوع ميزانية
          </Button>
        )}
      </div>

      <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
        <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالكود أو الاسم..." />
        <FilterSelect
          value={controlMethodFilter}
          onChange={setControlMethodFilter}
          options={controlMethodOptions}
          placeholder="طريقة التحكم"
          label="طريقة التحكم"
        />
        <FilterSelect
          value={isActiveFilter}
          onChange={setIsActiveFilter}
          options={isActiveOptions}
          placeholder="الحالة"
          label="الحالة"
        />
      </FilterBar>

      <div className="flex items-center gap-2 text-sm text-[var(--color-on-surface-variant)]">
        <span>{filtered.length} نتيجة</span>
        {items.length > 0 && <span>({items.length} إجمالي)</span>}
      </div>

      <DataGrid
        columns={allColumns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد نتائج"
        rowKey={(row) => row.id}
      />

      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={editItem ? 'تعديل نوع الميزانية' : 'إضافة نوع ميزانية جديد'}
        footer={
          <Button type="submit" form="budget-type-form" disabled={createMutation.isPending || updateMutation.isPending}>
            {editItem ? 'حفظ التعديلات' : 'إنشاء'}
          </Button>
        }
      >
        <form id="budget-type-form" onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="code" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">الكود *</label>
            <input
              id="code"
              name="code"
              type="text"
              required
              defaultValue={editItem?.code}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
          </div>
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">الاسم *</label>
            <input
              id="name"
              name="name"
              type="text"
              required
              defaultValue={editItem?.name}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
          </div>
          <div>
            <label htmlFor="description" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">الوصف</label>
            <textarea
              id="description"
              name="description"
              rows={3}
              defaultValue={editItem?.description}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
          </div>
          <div>
            <label htmlFor="controlMethod" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">طريقة التحكم *</label>
            <select
              id="controlMethod"
              name="controlMethod"
              required
              defaultValue={editItem?.controlMethod}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            >
              {Object.entries(budgetControlMethodLabels).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </div>
          <div className="flex items-center gap-2">
            <input
              id="allowOverrun"
              name="allowOverrun"
              type="checkbox"
              defaultChecked={editItem?.allowOverrun}
              className="w-4 h-4"
            />
            <label htmlFor="allowOverrun" className="text-sm text-[var(--color-on-surface)]">السماح بالتجاوز</label>
          </div>
        </form>
      </Dialog>

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        message={confirmToggle?.isActive ? 'هل تريد تعطيل هذا النوع؟' : 'هل تريد تنشيط هذا النوع؟'}
        title={confirmToggle?.isActive ? 'تعطيل نوع الميزانية' : 'تنشيط نوع الميزانية'}
        loading={toggleMutation.isPending}
      />
    </div>
  );
}
