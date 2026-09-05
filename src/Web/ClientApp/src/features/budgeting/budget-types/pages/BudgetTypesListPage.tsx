import { useMemo, useState } from 'react';
import { usePermission } from '@/shared/hooks/usePermission';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { Button, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Badge } from '@/components/ui';
import { Plus, Pencil } from 'lucide-react';
import { toast } from 'sonner';
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
          toast.success(confirmToggle.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح');
          setConfirmToggle(null);
        },
        onError: () => toast.error('حدث خطأ أثناء التبديل'),
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
            toast.success('تم التحديث بنجاح');
            setDialogOpen(false);
          },
          onError: () => toast.error('حدث خطأ أثناء التحديث'),
        },
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          toast.success('تم الإنشاء بنجاح');
          setDialogOpen(false);
        },
        onError: () => toast.error('حدث خطأ أثناء الإنشاء'),
      });
    }
  }

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

      <div className="w-full overflow-x-auto rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)]">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[var(--color-border-container)] bg-[var(--color-surface-container-low)]">
              <th className="px-4 py-3 text-right font-medium text-[var(--color-on-surface-variant)]">الكود</th>
              <th className="px-4 py-3 text-right font-medium text-[var(--color-on-surface-variant)]">الاسم</th>
              <th className="px-4 py-3 text-right font-medium text-[var(--color-on-surface-variant)]">طريقة التحكم</th>
              <th className="px-4 py-3 text-right font-medium text-[var(--color-on-surface-variant)]">السماح بالتجاوز</th>
              <th className="px-4 py-3 text-right font-medium text-[var(--color-on-surface-variant)]">الحالة</th>
              {canManage && <th className="px-4 py-3 text-right font-medium text-[var(--color-on-surface-variant)]">إجراءات</th>}
            </tr>
          </thead>
          <tbody>
            {filtered.map((item) => (
              <tr key={item.id} className="border-b border-[var(--color-border-container)] last:border-0 hover:bg-[var(--color-surface-container-low)] transition-colors">
                <td className="px-4 py-3 font-mono text-[var(--color-on-surface)]">{item.code}</td>
                <td className="px-4 py-3 text-[var(--color-on-surface)]">{item.name}</td>
                <td className="px-4 py-3">
                  <Badge variant={controlMethodBadgeVariant[item.controlMethod]}>
                    {budgetControlMethodLabels[item.controlMethod]}
                  </Badge>
                </td>
                <td className="px-4 py-3">
                  <Switch checked={item.allowOverrun} disabled />
                </td>
                <td className="px-4 py-3">
                  {canManage ? (
                    <Switch
                      checked={item.isActive}
                      onChange={() => handleToggle(item)}
                      label={item.isActive ? 'نشط' : 'معطل'}
                    />
                  ) : (
                    <Badge variant={item.isActive ? 'success' : 'danger'}>
                      {item.isActive ? 'نشط' : 'معطل'}
                    </Badge>
                  )}
                </td>
                {canManage && (
                  <td className="px-4 py-3">
                    <Button variant="ghost" size="icon" onClick={() => handleEdit(item)} aria-label="تعديل">
                      <Pencil size={16} />
                    </Button>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

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
