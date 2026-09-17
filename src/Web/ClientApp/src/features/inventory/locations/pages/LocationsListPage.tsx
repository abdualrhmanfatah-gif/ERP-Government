import { useMemo, useState } from 'react';
import { Page, Button, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Input, Select, Textarea, DataGrid } from '@/components/ui';
import { type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Pencil } from 'lucide-react';
import { useLocationsList, useCreateLocation, useUpdateLocation, useToggleLocationActive } from '../hooks/useLocations';
import type { Location } from '../hooks/useLocations';
import { notify } from '@/features/notifications/notify';

const isActiveOptions = [
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'غير نشط' },
];

function getDescendantIds(items: Location[], rootId: number): Set<number> {
  const childrenOf = new Map<number, Location[]>();
  for (const item of items) {
    if (item.parentLocationId == null) continue;
    const list = childrenOf.get(item.parentLocationId) ?? [];
    list.push(item);
    childrenOf.set(item.parentLocationId, list);
  }
  const result = new Set<number>([rootId]);
  const queue = [rootId];
  while (queue.length > 0) {
    const current = queue.pop()!;
    for (const child of childrenOf.get(current) ?? []) {
      if (!result.has(child.id)) {
        result.add(child.id);
        queue.push(child.id);
      }
    }
  }
  return result;
}

export default function LocationsListPage() {
  const [search, setSearch] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<Location | null>(null);
  const [formCode, setFormCode] = useState('');
  const [formName, setFormName] = useState('');
  const [formBarcode, setFormBarcode] = useState('');
  const [formParentId, setFormParentId] = useState('');
  const [formCity, setFormCity] = useState('');
  const [formAddress, setFormAddress] = useState('');
  const [formCapacity, setFormCapacity] = useState('');
  const [formIsActive, setFormIsActive] = useState(true);
  const [confirmToggle, setConfirmToggle] = useState<{ item: Location; activate: boolean } | null>(null);

  const { data: locations = [], isLoading, error, refetch } = useLocationsList();
  const createMutation = useCreateLocation();
  const updateMutation = useUpdateLocation();
  const toggleMutation = useToggleLocationActive();

  const filtered = useMemo(() => {
    return locations.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        const hay = `${item.code} ${item.name} ${item.city ?? ''}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }
      if (isActiveFilter && (isActiveFilter === 'true') !== item.isActive) return false;
      return true;
    });
  }, [locations, search, isActiveFilter]);

  const hasFilters = !!search || !!isActiveFilter;

  function handleClearFilters() {
    setSearch('');
    setIsActiveFilter('');
  }

  function handleCreate() {
    setEditItem(null);
    setFormCode('');
    setFormName('');
    setFormBarcode('');
    setFormParentId('');
    setFormCity('');
    setFormAddress('');
    setFormCapacity('');
    setFormIsActive(true);
    setDialogOpen(true);
  }

  function handleEdit(item: Location) {
    setEditItem(item);
    setFormCode(item.code);
    setFormName(item.name);
    setFormBarcode(item.barcode ?? '');
    setFormParentId(item.parentLocationId != null ? String(item.parentLocationId) : '');
    setFormCity(item.city ?? '');
    setFormAddress(item.address ?? '');
    setFormCapacity(item.capacity != null ? String(item.capacity) : '');
    setFormIsActive(item.isActive);
    setDialogOpen(true);
  }

  function handleToggle(item: Location, activate: boolean) {
    setConfirmToggle({ item, activate });
  }

  function confirmToggleAction() {
    if (!confirmToggle) return;
    const { item, activate } = confirmToggle;
    toggleMutation.mutate(
      { id: item.id, activate, rowVersion: item.rowVersion },
      {
        onSuccess: () => {
          notify({ type: 'success', title: activate ? 'تم التنشيط بنجاح' : 'تم التعطيل بنجاح' });
          setConfirmToggle(null);
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التبديل' }),
      },
    );
  }

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!formCode.trim() || !formName.trim()) return;

    const data = {
      code: formCode.trim(),
      name: formName.trim(),
      barcode: formBarcode.trim() || undefined,
      parentLocationId: formParentId ? Number(formParentId) : undefined,
      city: formCity.trim() || undefined,
      address: formAddress.trim() || undefined,
      capacity: formCapacity ? Number(formCapacity) : undefined,
      isActive: formIsActive,
    };

    if (editItem) {
      updateMutation.mutate(
        { id: editItem.id, data: { ...data, rowVersion: editItem.rowVersion } },
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

  const parentOptions = useMemo(() => {
    const excluded = editItem ? getDescendantIds(locations, editItem.id) : new Set<number>();
    return locations
      .filter((item) => !excluded.has(item.id))
      .map((item) => ({
        value: String(item.id),
        label: item.breadcrumb ? `${item.breadcrumb}/${item.code} - ${item.name}` : `${item.code} - ${item.name}`,
      }));
  }, [locations, editItem]);

  const columns: DataGridColumn<Location>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono">{row.code}</span> },
    {
      header: 'الاسم',
      cell: (row) => (
        <span style={{ paddingInlineStart: `${(row.level ?? 0) * 1.25}rem` }}>
          {(row.level ?? 0) > 0 && <span aria-hidden="true">↳ </span>}
          {row.name}
        </span>
      ),
    },
    { header: 'الموقع الأب', cell: (row) => row.parentName ?? '—' },
    { header: 'المدينة', cell: (row) => row.city ?? '—' },
    { header: 'الباركود', cell: (row) => row.barcode ?? '—' },
    {
      header: 'الحالة',
      cell: (row) => (
        <Switch
          checked={row.isActive}
          onChange={() => handleToggle(row, !row.isActive)}
          label={row.isActive ? 'نشط' : 'غير نشط'}
        />
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
      title="المواقع"
      description="إدارة المواقع المخزنية (شجرة)"
      loading={isLoading}
      error={error?.message}
      onRetry={() => refetch()}
      actions={
        <Button onClick={handleCreate} icon={<Plus size={16} />}>
          إضافة موقع
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالكود أو الاسم أو المدينة..." />
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
        emptyMessage="لا توجد مواقع بعد"
        rowKey={(row) => row.id}
      />

      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={editItem ? 'تعديل الموقع' : 'إضافة موقع جديد'}
        footer={
          <Button type="submit" form="location-form" disabled={createMutation.isPending || updateMutation.isPending} loading={createMutation.isPending || updateMutation.isPending}>
            {editItem ? 'حفظ التعديلات' : 'إنشاء'}
          </Button>
        }
      >
        <form id="location-form" onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج الموقع">
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
          <Select
            label="الموقع الأب"
            value={formParentId}
            onChange={(e) => setFormParentId(e.target.value)}
            options={[{ value: '', label: 'بدون أب (موقع رئيسي)' }, ...parentOptions]}
          />
          <Input
            label="المدينة"
            value={formCity}
            onChange={(e) => setFormCity(e.target.value)}
          />
          <Textarea
            label="العنوان"
            value={formAddress}
            onChange={(e) => setFormAddress(e.target.value)}
            rows={2}
          />
          <Input
            label="الباركود"
            value={formBarcode}
            onChange={(e) => setFormBarcode(e.target.value)}
          />
          <Input
            label="السعة"
            type="number"
            step="0.01"
            value={formCapacity}
            onChange={(e) => setFormCapacity(e.target.value)}
          />
          {!editItem && (
            <Switch
              label="نشط"
              checked={formIsActive}
              onChange={setFormIsActive}
            />
          )}
        </form>
      </Dialog>

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        message={confirmToggle?.activate ? 'هل تريد تنشيط هذا الموقع؟' : 'هل تريد تعطيل هذا الموقع؟ سيتم الرفض إذا كان مرتبطاً بأصول أو له مواقع فرعية نشطة.'}
        title={confirmToggle?.activate ? 'تنشيط الموقع' : 'تعطيل الموقع'}
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
