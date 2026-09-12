import { useMemo, useState } from 'react';
import { Page, Button, Badge, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Input, DataGrid } from '@/components/ui';
import { type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Pencil } from 'lucide-react';
import { useItemCategoriesList, useCreateItemCategory, useUpdateItemCategory, useToggleItemCategoryActive } from '../hooks/useItemCategories';
import type { ItemCategory } from '../hooks/useItemCategories';
import { notify } from '@/features/notifications/notify';

const isActiveOptions = [
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'غير نشط' },
];

export default function ItemCategoriesListPage() {
  const [search, setSearch] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<ItemCategory | null>(null);
  const [formCode, setFormCode] = useState('');
  const [formName, setFormName] = useState('');
  const [formNameEn, setFormNameEn] = useState('');
  const [formIsActive, setFormIsActive] = useState(true);
  const [confirmToggle, setConfirmToggle] = useState<ItemCategory | null>(null);

  const { data: categories = [], isLoading, error, refetch } = useItemCategoriesList();
  const createMutation = useCreateItemCategory();
  const updateMutation = useUpdateItemCategory();
  const toggleMutation = useToggleItemCategoryActive();

  const filtered = useMemo(() => {
    return categories.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.code.toLowerCase().includes(q) && !item.name.toLowerCase().includes(q)) return false;
      }
      if (isActiveFilter && (isActiveFilter === 'true') !== item.isActive) return false;
      return true;
    });
  }, [categories, search, isActiveFilter]);

  const hasFilters = !!search || !!isActiveFilter;

  function handleClearFilters() {
    setSearch('');
    setIsActiveFilter('');
  }

  function handleCreate() {
    setEditItem(null);
    setFormCode('');
    setFormName('');
    setFormNameEn('');
    setFormIsActive(true);
    setDialogOpen(true);
  }

  function handleEdit(item: ItemCategory) {
    setEditItem(item);
    setFormCode(item.code);
    setFormName(item.name);
    setFormNameEn(item.nameEn ?? '');
    setFormIsActive(item.isActive);
    setDialogOpen(true);
  }

  function handleToggle(item: ItemCategory) {
    setConfirmToggle(item);
  }

  function confirmToggleAction() {
    if (!confirmToggle) return;
    toggleMutation.mutate(confirmToggle.id, {
      onSuccess: () => {
        notify({ type: 'success', title: confirmToggle.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح' });
        setConfirmToggle(null);
      },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التبديل' }),
    });
  }

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!formCode.trim() || !formName.trim()) return;

    const data = {
      code: formCode.trim(),
      name: formName.trim(),
      nameEn: formNameEn.trim() || undefined,
      isActive: formIsActive,
    };

    if (editItem) {
      updateMutation.mutate(
        { id: editItem.id, data },
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

  const columns: DataGridColumn<ItemCategory>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono">{row.code}</span> },
    { header: 'الاسم', cell: (row) => row.name },
    { header: 'الاسم الإنجليزي', cell: (row) => row.nameEn ?? '—' },
    { header: 'الأب', cell: (row) => row.parentName ?? '—' },
    { header: 'المستوى', cell: (row) => row.level != null ? <Badge variant="primary">مستوى {row.level}</Badge> : '—' },
    {
      header: 'الحالة',
      cell: (row) => (
        <Switch checked={row.isActive} onChange={() => handleToggle(row)} label={row.isActive ? 'نشط' : 'غير نشط'} />
      ),
    },
    {
      header: 'إجراءات',
      cell: (row) => (
        <Button variant="ghost" size="icon" onClick={() => handleEdit(row)} aria-label="تعديل">
          <Pencil size={16} />
        </Button>
      ),
    },
  ];

  return (
    <Page
      title="تصنيفات الأصناف"
      description="إدارة تصنيفات الأصناف"
      loading={isLoading}
      error={error?.message}
      onRetry={() => refetch()}
      actions={
        <Button onClick={handleCreate} icon={<Plus size={16} />}>
          إضافة تصنيف
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالكود أو الاسم..." />
          <FilterSelect
            value={isActiveFilter}
            onChange={setIsActiveFilter}
            options={isActiveOptions}
            placeholder="الحالة"
            label="الحالة"
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد تصنيفات بعد"
        rowKey={(row) => row.id}
      />

      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={editItem ? 'تعديل التصنيف' : 'إضافة تصنيف جديد'}
        footer={
          <Button type="submit" form="category-form" disabled={createMutation.isPending || updateMutation.isPending} loading={createMutation.isPending || updateMutation.isPending}>
            {editItem ? 'حفظ التعديلات' : 'إنشاء'}
          </Button>
        }
      >
        <form id="category-form" onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج التصنيف">
          <Input
            label="الكود *"
            value={formCode}
            onChange={(e) => setFormCode(e.target.value)}
            required
          />
          <Input
            label="الاسم *"
            value={formName}
            onChange={(e) => setFormName(e.target.value)}
            required
          />
          <Input
            label="الاسم الإنجليزي"
            value={formNameEn}
            onChange={(e) => setFormNameEn(e.target.value)}
          />
          <Switch
            label="نشط"
            checked={formIsActive}
            onChange={setFormIsActive}
          />
        </form>
      </Dialog>

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        message={confirmToggle?.isActive ? 'هل تريد تعطيل هذا التصنيف؟' : 'هل تريد تنشيط هذا التصنيف؟'}
        title={confirmToggle?.isActive ? 'تعطيل التصنيف' : 'تنشيط التصنيف'}
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
