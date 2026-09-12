import { useMemo, useState } from 'react';
import { Page, Button, Badge, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Input, Select, DataGrid } from '@/components/ui';
import { type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Pencil } from 'lucide-react';
import { useUnitsList, useCreateUnit, useUpdateUnit, useToggleUnitActive } from '../hooks/useUnits';
import type { Unit } from '../hooks/useUnits';
import { notify } from '@/features/notifications/notify';

const unitTypeOptions = [
  { value: 'Weight', label: 'وزن' },
  { value: 'Volume', label: 'حجم' },
  { value: 'Length', label: 'طول' },
  { value: 'Count', label: 'عدد' },
  { value: 'Area', label: 'مساحة' },
];

const isActiveOptions = [
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'غير نشط' },
];

export default function UnitsListPage() {
  const [search, setSearch] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<Unit | null>(null);
  const [formCode, setFormCode] = useState('');
  const [formName, setFormName] = useState('');
  const [formUnitType, setFormUnitType] = useState('');
  const [formBaseUnitId, setFormBaseUnitId] = useState('');
  const [formConversionToBase, setFormConversionToBase] = useState('');
  const [formIsActive, setFormIsActive] = useState(true);
  const [confirmToggle, setConfirmToggle] = useState<Unit | null>(null);

  const { data: units = [], isLoading, error, refetch } = useUnitsList();
  const createMutation = useCreateUnit();
  const updateMutation = useUpdateUnit();
  const toggleMutation = useToggleUnitActive();

  const filtered = useMemo(() => {
    return units.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.code.toLowerCase().includes(q) && !item.name.toLowerCase().includes(q)) return false;
      }
      if (isActiveFilter && (isActiveFilter === 'true') !== item.isActive) return false;
      return true;
    });
  }, [units, search, isActiveFilter]);

  const hasFilters = !!search || !!isActiveFilter;

  function handleClearFilters() {
    setSearch('');
    setIsActiveFilter('');
  }

  function handleCreate() {
    setEditItem(null);
    setFormCode('');
    setFormName('');
    setFormUnitType('');
    setFormBaseUnitId('');
    setFormConversionToBase('');
    setFormIsActive(true);
    setDialogOpen(true);
  }

  function handleEdit(item: Unit) {
    setEditItem(item);
    setFormCode(item.code);
    setFormName(item.name);
    setFormUnitType(item.unitType ?? '');
    setFormBaseUnitId(item.baseUnitId ? String(item.baseUnitId) : '');
    setFormConversionToBase(item.conversionToBase ? String(item.conversionToBase) : '');
    setFormIsActive(item.isActive);
    setDialogOpen(true);
  }

  function handleToggle(item: Unit) {
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
      unitType: formUnitType || undefined,
      baseUnitId: formBaseUnitId ? Number(formBaseUnitId) : undefined,
      conversionToBase: formConversionToBase ? Number(formConversionToBase) : undefined,
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

  const columns: DataGridColumn<Unit>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono">{row.code}</span> },
    { header: 'الاسم', cell: (row) => row.name },
    { header: 'النوع', cell: (row) => row.unitType ? <Badge variant="outline">{row.unitType}</Badge> : '—' },
    { header: 'الوحدة الأساسية', cell: (row) => row.baseUnitName ?? '—' },
    { header: 'معامل التحويل', cell: (row) => row.conversionToBase != null ? <span className="tabular-nums">{row.conversionToBase}</span> : '—', align: 'right' },
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
      title="وحدات القياس"
      description="إدارة وحدات القياس"
      loading={isLoading}
      error={error?.message}
      onRetry={() => refetch()}
      actions={
        <Button onClick={handleCreate} icon={<Plus size={16} />}>
          إضافة وحدة
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
        emptyMessage="لا توجد وحدات بعد"
        rowKey={(row) => row.id}
      />

      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={editItem ? 'تعديل الوحدة' : 'إضافة وحدة جديدة'}
        footer={
          <Button type="submit" form="unit-form" disabled={createMutation.isPending || updateMutation.isPending}>
            {editItem ? 'حفظ التعديلات' : 'إنشاء'}
          </Button>
        }
      >
        <form id="unit-form" onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج الوحدة">
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
            label="نوع الوحدة"
            value={formUnitType}
            onChange={(e) => setFormUnitType(e.target.value)}
            options={unitTypeOptions}
          />
          <Select
            label="الوحدة الأساسية"
            value={formBaseUnitId}
            onChange={(e) => setFormBaseUnitId(e.target.value)}
            options={units.filter((u) => !editItem || u.id !== editItem.id).map((u) => ({ value: String(u.id), label: u.name }))}
          />
          {formBaseUnitId && (
            <Input
              label="معامل التحويل"
              type="number"
              step="0.0001"
              value={formConversionToBase}
              onChange={(e) => setFormConversionToBase(e.target.value)}
            />
          )}
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
        message={confirmToggle?.isActive ? 'هل تريد تعطيل هذه الوحدة؟' : 'هل تريد تنشيط هذه الوحدة؟'}
        title={confirmToggle?.isActive ? 'تعطيل الوحدة' : 'تنشيط الوحدة'}
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
